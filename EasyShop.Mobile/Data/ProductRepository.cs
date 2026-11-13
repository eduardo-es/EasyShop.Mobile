using EasyShop.Mobile.Exceptions;
using EasyShop.Mobile.Interfaces;
using EasyShop.Mobile.Models;
using LiteDB;

namespace EasyShop.Mobile.Data;

/// <summary>
/// Repositório LiteDB do catálogo de produtos.
/// O arquivo products.db é criado exclusivamente em <see cref="FileSystem.AppDataDirectory"/>,
/// diretório privado e seguro da aplicação no dispositivo.
/// </summary>
public sealed class ProductRepository : IProductRepository, IDisposable
{
    private const string DatabaseFileName = "products.db";
    private const string CollectionName = "products";

    private readonly SemaphoreSlim _gate = new(1, 1);
    private LiteDatabase? _database;
    private ILiteCollection<Product>? _products;
    private bool _disposed;

    /// <inheritdoc />
    public async Task<Product> Insert(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);
        ThrowIfDisposed();

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await Task.Run(() =>
            {
                var collection = GetCollection();

                // Garante um documento válido mesmo quando o repositório é chamado
                // diretamente por outra camada, sem passar pelo Service.
                product.Attributes ??= new Dictionary<string, object>();

                // O LiteDB gera o Id; um Id residual nunca deve ser reaproveitado.
                product.Id = 0;
                collection.Insert(product);
                return product;
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (ProductPersistenceException)
        {
            throw;
        }
        catch (Exception ex) when (ex is LiteException or IOException or UnauthorizedAccessException)
        {
            throw new ProductPersistenceException(
                "Não foi possível salvar o produto no banco de dados local.", ex);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Product>> GetAll(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await Task.Run<IReadOnlyList<Product>>(
                () => GetCollection()
                    .Query()
                    .OrderByDescending(product => product.Id)
                    .ToList(),
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (ProductPersistenceException)
        {
            throw;
        }
        catch (Exception ex) when (ex is LiteException or IOException or UnauthorizedAccessException)
        {
            throw new ProductPersistenceException(
                "Não foi possível carregar os produtos do banco de dados local.", ex);
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _database?.Dispose();
        _database = null;
        _products = null;
        _gate.Dispose();
    }

    /// <summary>
    /// Abre a conexão com products.db na primeira operação. A inicialização tardia
    /// evita acessar o armazenamento do dispositivo durante a construção do container DI.
    /// Deve ser chamada apenas dentro do semáforo de acesso.
    /// </summary>
    private ILiteCollection<Product> GetCollection()
    {
        if (_products is not null)
        {
            return _products;
        }

        try
        {
            var databasePath = Path.Combine(FileSystem.AppDataDirectory, DatabaseFileName);

            var connectionString = new ConnectionString
            {
                Filename = databasePath,
                Connection = ConnectionType.Shared,
                Upgrade = true
            };

            _database = new LiteDatabase(connectionString);
            _products = _database.GetCollection<Product>(CollectionName);
            _products.EnsureIndex(product => product.Name);

            return _products;
        }
        catch (Exception ex) when (ex is LiteException or IOException or UnauthorizedAccessException)
        {
            _database?.Dispose();
            _database = null;
            _products = null;

            throw new ProductPersistenceException(
                "Não foi possível inicializar o banco de dados local de produtos.", ex);
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}

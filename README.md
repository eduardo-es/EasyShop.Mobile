# 📱 EasyShop - Catálogo com Persistência Poliglota (.NET MAUI)

Este projeto foi desenvolvido como parte da atividade académica de Persistência Poliglota, com o objetivo de criar um aplicativo mobile utilizando **.NET MAUI** que demonstra o uso de duas abordagens de banco de dados locais operando em conjunto na mesma aplicação.

## 👥 Integrantes do Grupo
* **Eduardo Mendes Pereira**
* **Gabriel Tavarez**

## 🏗️ Arquitetura e Persistência Poliglota
O aplicativo é um sistema enxuto dividido em duas abas principais, focado em validar as operações de inclusão e listagem em dois paradigmas de dados distintos:

* 📦 **Aba de Produtos (NoSQL com LiteDB):**
  * Ideal para suportar atributos dinâmicos dos produtos.
  * Funcionalidades: Formulário de inclusão (nome, preço e atributos opcionais) e listagem dinâmica.
* 👥 **Aba de Clientes (SQL com SQLite):**
  * Ideal para dados estruturados e relacionais.
  * Funcionalidades: Formulário de inclusão (nome, e-mail) e listagem estruturada.

## 🚀 Como executar o projeto localmente

### Pré-requisitos
* [Visual Studio 2022](https://visualstudio.microsoft.com/) com a carga de trabalho **.NET Multi-platform App UI development (.NET MAUI)** instalada.
* Emulador Android configurado e a funcionar no Visual Studio.

### Passos para execução
1. Faça o clone deste repositório para a sua máquina:
   ```bash
   git clone [https://github.com/eduardo-es/EasyShop.Mobile.git](https://github.com/eduardo-es/EasyShop.Mobile.git)

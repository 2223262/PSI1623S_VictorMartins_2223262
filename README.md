# DigiAirlines - Sistema de Gestão de Reservas de Voos

**Autor:** Victor Hugo Cristóvão Martins (Nº 2223262)  
**Curso:** TGPSI-S (PSI1623S)

![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoft%20sql%20server&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)

## Descrição do Projeto

**DigiAirlines** é uma aplicação de ambiente de trabalho (desktop) completa, desenvolvida em C# com Windows Forms e uma base de dados SQL Server. O projeto foi desenhado para modernizar e simplificar o processo de reserva de voos, oferecendo uma solução integrada para clientes e administradores.

A plataforma permite que os clientes criem contas, pesquisem destinos, efetuem e giram as suas próprias reservas. Para a gestão, o sistema incorpora um painel de administração robusto que permite o controlo centralizado de clientes, destinos e a monitorização de todas as operações de reserva.

## Funcionalidades Principais

A aplicação está dividida em dois grandes fluxos, com funcionalidades específicas para cada tipo de perfil.

#### Para Clientes:
-   **Autenticação e Gestão de Perfil:** Criação de conta segura e um painel de perfil para alterar a senha.
-   **Menu Principal Interativo:** Um dashboard de boas-vindas com o nome do utilizador e um relógio em tempo real.
-   **Sistema de Reserva Avançado:** Permite pesquisar destinos com uma funcionalidade de "auto-predict", selecionar datas de ida e volta, e escolher a classe do voo.
-   **Cálculo de Preço Dinâmico:** O preço final é ajustado automaticamente com base na classe de voo selecionada (+25% para Executiva, +50% para Primeira-classe).
-   **Histórico e Gestão de Reservas:** Os clientes podem visualizar o seu histórico, cancelar voos futuros ou editá-los (alterar classe e data), com aplicação de uma taxa de alteração.
-   **Geração de Recibo:** Após cada compra, é gerado um recibo detalhado que pode ser guardado localmente num ficheiro `.txt`.

#### Para Administradores:
-   **Painel de Controlo Centralizado:** Um dashboard que oferece uma visão geral do sistema e acesso a todas as funcionalidades de gestão.
-   **Gestão de Clientes (CRUD):** O administrador pode visualizar, adicionar e apagar contas de clientes.
-   **Gestão de Destinos (CRUD):** Controlo total para adicionar novos destinos com os seus preços base e apagar os existentes.
-   **Monitorização:** Acesso a um histórico completo de todas as reservas efetuadas no sistema.
-   **Estatísticas Básicas:** Visualização rápida do número total de clientes, reservas e destinos.

## Tecnologias Utilizadas
* **Linguagem:** C# (.NET Framework 4.7.2)
* **Interface:** Windows Forms
* **Base de Dados:** SQL Server (LocalDB)
* **Acesso a Dados:** ADO.NET
* **Bibliotecas Externas:**
    * **Guna.UI2.WinForms:** Para a criação de uma interface de utilizador moderna e apelativa.
    * **Microsoft.VisualBasic:** Para a utilização do `Interaction.InputBox` no painel de administração.

## Como Começar

Para executar este projeto no seu ambiente local, siga estes passos:

### 1. Pré-requisitos
* **Visual Studio 2022** (ou superior) com o workload ".NET Desktop Development".
* **SQL Server** (qualquer edição, como Express ou Developer) e **SQL Server Management Studio (SSMS)**.

### 2. Instalação e Configuração

1.  **Clone o repositório:**
    ```bash
    git clone [https://github.com/2223262/PSI1623S_VictorMartins_2223262.git](https://github.com/2223262/PSI1623S_VictorMartins_2223262.git)
    ```
2.  **Aceda à pasta do projeto:**
    ```bash
    cd PSI1623S_VictorMartins_2223262
    ```
3.  **Configure a Base de Dados (Passo Essencial):**
    * Abra o **SQL Server Management Studio (SSMS)**.
    * Abra o ficheiro `SCRIPT_COMPLETO_BD.sql` que se encontra na raiz do projeto.
    * Execute o script na sua totalidade (pressione F5 ou clique em "Execute"). Este script irá criar a base de dados `DigiAirlines`, todas as tabelas e os dados iniciais necessários.
    * Uma conta de **admin** será criada automaticamente para que possa testar:
        * **Utilizador:** `admin`
        * **Senha:** `admin123`

4.  **Execute a Aplicação:**
    * Abra o ficheiro da solução (`@DigiAirlines.sln`) com o Visual Studio.
    * Compile e execute o projeto pressionando **F5**. A aplicação deverá iniciar no ecrã de login.

---
> Projeto desenvolvido por **Victor Martins** para a unidade curricular TGPSI23-S.

# 🚀 Sistema de Enquetes (Survey System)

## 1. APRESENTAÇÃO DO PROJETO

Este projeto é um sistema de gestão de enquetes desenvolvido com ASP.NET Core 9, seguindo os princípios do Domain-Driven Design (DDD). Ele oferece uma API RESTful para realizar operações CRUD (Create, Read, Update, Delete) em dados de enquetes, incluindo título, descrição, período e as perguntas que a compõem e também permitindo a submissão de respostas e a captura do resultado.

### Objetivo Principal e Escopo
O objetivo principal deste projeto é demonstrar a aplicação prática de conceitos avançados de arquitetura de software que foram estudados durante a disciplina "Arquitetura .NET" do "MIT em Arquitetura de Software", como DDD, padrões de projeto e Entity Framework Core, em um cenário de negócios comum: um sistema para enquete/questionário. O escopo abrange o sistema de questionários online que terá como finalidade principal a elaboração de pesquisas públicas. Um dos alvos é pesquisa pública sobre as eleições, onde seriam feitos anúncios em diversas redes sociais convidando as pessoas a responderem a pesquisa. O questionário teria uma estrutura simples de perguntas com resposta no modelo múltipla escolha. Como o alvo das pesquisas são milhões de pessoas, é preciso se preocupar com questões de escala também. Depois do período de coleta de respostas, o sistema deve disponibilizar de forma sumarizada, para alguns usuários, o resultado da pesquisa.

### O que o Projeto Faz
O sistema permite:
- **Cadastrar enquetes** com informações básicas e múltiplas perguntas.
- **Atualizar enquetes**, incluindo suas perguntas.
- **Consultar enquetes** individualmente ou em lista.
- **Publicar enquete**.
- **Submeter respostas de maneira anônima**.
- **Consultar submissões** individualmente ou em lista.
- **Capturar o resultada das enquetes** individualmente, onde também é apresentado o resultado com quantidade de votos e percentual.
- **Encerrar enquete**.
- **Deletar enquete**.
- **Deletar todas enquetes**.
- **Deletar todas submissões**.

### Para Quem É
Este projeto é ideal para:
- **Estudantes e desenvolvedores** que desejam aprofundar seus conhecimentos em ASP.NET Core, Entity Framework Core e, principalmente, Domain-Driven Design.
- **Professores e instrutores** como material didático para demonstrar boas práticas de arquitetura e desenvolvimento de software.
- **Equipes de desenvolvimento** que buscam um exemplo claro de como estruturar uma aplicação com DDD.

### Por Que Foi Criado
Foi criado para validar os conceitos que aprendi durante aula, onde abordamos diversos conceitos de arquitetura de software aplicado a plataforma .Net. Foi dada atenção especial a importancia de DDD (Domain-Driven-Design) e como ele afeta toda a solução desenvolvida, o que certamente foi o maior desafio deste projeto. Além disso, este projeto aplica padrões de repositório e melhores práticas de organização da solution.

## 2. ARQUITETURA E DESIGN

A arquitetura do projeto segue o padrão de **Arquitetura em Camadas (Layered Architecture)**, com forte influência do **Domain-Driven Design (DDD)**. Isso garante uma separação clara de responsabilidades, facilitando a manutenção, testabilidade e escalabilidade da aplicação.

### Explicação Completa da Arquitetura em Camadas

#### 1. **SurveySystem.Domain (Camada de Domínio)**
- **Coração da aplicação.** Contém a lógica de negócios, entidades, Value Objects, agregados e interfaces de repositório.
- **Independente de qualquer tecnologia de infraestrutura ou UI.** Não conhece banco de dados, frameworks web, etc.
- **Foco:** Modelar o problema de negócio de forma rica e expressiva.

#### 2. **SurveySystem.Infrastructure.Data (Camada de Infraestrutura)**
- **Responsável pela persistência de dados e outras preocupações técnicas.**
- Implementa as interfaces de repositório definidas na camada de Domínio.
- Utiliza Entity Framework Core para interagir com o banco de dados (SQL Server LocalDB).
- Contém configurações de mapeamento de entidades para o banco de dados.

#### 3. **SurveySystem.API (Camada de Apresentação/Aplicação)**
- **Ponto de entrada da aplicação.** Expõe a funcionalidade de negócio através de uma API RESTful.
- Contém controladores (Controllers) que recebem requisições HTTP, orquestram as operações de domínio e retornam respostas HTTP.
- Utiliza DTOs (Data Transfer Objects) para desacoplar a API do modelo de domínio.
- Configura a injeção de dependência e o pipeline da aplicação (middleware).

#### 4. **SurveySystem.Infrastructure.Tests (Camada de Testes de Infraestrutura)**
- Contém testes unitários para a implementação do repositório, garantindo que a persistência de dados funcione corretamente.

#### 5. **SurveySystem.Domain.Tests (Camada de Testes de Domínio)**
- Contém testes unitários para as entidades e Value Objects do domínio, garantindo que a lógica de negócio esteja correta e robusta.

### Padrões de Projeto Utilizados

-   **Domain-Driven Design (DDD)**: Foco na modelagem do domínio de negócio, com linguagem ubíqua e conceitos de Aggregate Roots, Value Objects e Repositories.
-   **Repository Pattern**: Abstrai a lógica de persistência de dados, permitindo que a camada de domínio trabalhe com coleções de objetos sem se preocupar com os detalhes do armazenamento.
-   **Factory Pattern**: Utilizado nos métodos `Create` dos Value Objects e Aggregate Roots para encapsular a lógica de criação e validação, garantindo que os objetos sejam sempre criados em um estado válido.
-   **Value Object Pattern**: Objetos que representam um conceito descritivo no domínio, definidos pela sua composição de atributos e comparados por valor, não por identidade. São imutáveis.
-   **Aggregate Root Pattern**: Entidades que são a raiz de um cluster de objetos (Aggregate), garantindo a consistência transacional dentro do agregado. Todas as operações externas devem passar pela Aggregate Root.
-   **Dependency Injection (DI)**: Utilizado para gerenciar as dependências entre as camadas e componentes, promovendo o baixo acoplamento e a testabilidade.
-   **Fluent API (EF Core)**: Usada para configurar o mapeamento objeto-relacional no Entity Framework Core, permitindo mapear Value Objects complexos para o banco de dados.
-   **RESTful API**: A camada de API segue os princípios REST para comunicação entre cliente e servidor, utilizando verbos HTTP e URLs semânticas.

### Fluxo de Dados Completo

1.  **Requisição HTTP (API)**: Um cliente (ex: frontend, Postman) envia uma requisição HTTP (POST, GET, PUT, DELETE) para um endpoint da `SurveySystem.API`.
2.  **Controller (API)**: O `SurveyController` ou `SubmissionController`  recebe a requisição, valida os DTOs de entrada e, se necessário, converte-os para o formato esperado pelo domínio.
3.  **Serviço de Aplicação (API/Domínio)**: O Controller invoca métodos na camada de Domínio (através da interface do repositório) para executar a lógica de negócio.
4.  **Aggregate Root (Domínio)**: O `Survey` ou `Submission` (Aggregate Root) executa as regras de negócio, manipula seus Value Objects (`SurveyStatus`, `SurveyPerior`, `Question`, `Option` para `Survey` e `Answer` para `Submission`) e garante a consistência interna.
5.  **Repositório (Domínio/Infraestrutura)**: As interfaces `ISurveyRepository` e `ISubmissionRepository` é invocada. As implementações `SqlServerSurveyRepository` e `SqlServerSubmissionRepository`, respectivamente (na camada de Infraestrutura), traduz as operações de domínio em operações de banco de dados.
6.  **Entity Framework Core (Infraestrutura)**: O EF Core, usando o `SurveySystemDbContext` e as configurações da `CustomerConfiguration`, interage com o SQL Server LocalDB para persistir ou recuperar os dados.
7.  **Resposta (Infraestrutura/Domínio/API)**: Os dados são retornados do banco, convertidos de volta para objetos de domínio, e então para DTOs de resposta pela API, que são enviados de volta ao cliente como uma resposta HTTP.

### Decisões importantes ao longo do projeto

1. ... 
2. ...
3. ...

### Diagrama da Arquitetura

TO DO
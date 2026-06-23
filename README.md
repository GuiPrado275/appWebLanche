# 🍔 São Judas Lanches

Sistema web de pedidos para lanchonete, desenvolvido em **ASP.NET Core MVC (.NET 6)**. A aplicação resolve um problema real de pequenos estabelecimentos: a dificuldade de manter a organização dos pedidos em momentos de pico, substituindo o controle manual por um fluxo digital completo — do cadastro do cliente até a entrega do pedido — com três níveis de permissão (Cliente, Funcionário e Administrador).

> Projeto acadêmico/autoral, construído do zero com C#, Entity Framework Core e SQL Server, utilizando o padrão arquitetural **MVC**.

---

## 📑 Sumário

- [Sobre o projeto](#-sobre-o-projeto)
- [Funcionalidades](#-funcionalidades)
- [Perfis de usuário e permissões](#-perfis-de-usuário-e-permissões)
- [Fluxo de um pedido](#-fluxo-de-um-pedido)
- [Tecnologias utilizadas](#-tecnologias-utilizadas)
- [Arquitetura e estrutura do projeto](#-arquitetura-e-estrutura-do-projeto)
- [Modelo de dados](#-modelo-de-dados)
- [Decisões técnicas de destaque](#-decisões-técnicas-de-destaque)
- [Como executar o projeto](#-como-executar-o-projeto)
- [Usuário administrador padrão](#-usuário-administrador-padrão)
- [Validações e regras de negócio](#-validações-e-regras-de-negócio)
- [Limitações conhecidas](#-limitações-conhecidas--próximos-passos)
- [Autor](#-autor)

---

## 📖 Sobre o projeto

O **São Judas Lanches** é um sistema de pedidos online para uma lanchonete fictícia localizada em São José do Rio Preto/SP. Ele permite que clientes montem pedidos a partir de um cardápio dinâmico, escolham endereço de entrega e forma de pagamento, e acompanhem o status do pedido em tempo real — enquanto a equipe da lanchonete (funcionários e administradores) gerencia cardápio, estoque, usuários e o andamento de cada pedido.

O projeto foi pensado para ir além de um CRUD simples, incluindo controle de estoque automático, regras de autorização por perfil, sistema de avaliação de pedidos e proteções contra manipulação de requisições (ex.: um cliente tentando avaliar o pedido de outra pessoa).

---

## ✨ Funcionalidades

### 🔐 Autenticação e conta
- Cadastro de novos clientes (`/Auth/Cadastro`), com validação de e-mail único e senha mínima de 6 caracteres.
- Login por e-mail e senha (`/Auth/Login`), com criação de sessão (`UsuarioId`, `NomeUsuario`, `PerfilUsuario`).
- Logout com limpeza total da sessão.
- Proteção contra "voltar pelo navegador": páginas autenticadas (e login/logout) são marcadas com cabeçalhos `no-store, no-cache` para impedir que o navegador exiba conteúdo em cache após o logout.
- Alteração de senha pelo próprio usuário, com confirmação da senha atual.
- Exclusão da própria conta (exceto para Administradores).

### 🍟 Cardápio (gestão de produtos)
- Listagem do cardápio agrupado por categoria e ordenado por nome, visível a todos os usuários logados.
- Cadastro, edição e exclusão de itens — **restrito ao Administrador**.
- Cada item possui nome, descrição, preço, categoria, estoque atual e disponibilidade.
- Atualização rápida de estoque por item, com desativação automática (`Disponivel = false`) quando o estoque chega a zero.
- Validações de modelo (nome obrigatório, preço entre R$ 0,01 e R$ 9.999,99, estoque não-negativo).

### 🛒 Pedidos
- Tela de registro de pedido (somente Cliente) com seleção de quantidades por item do cardápio, escolha de endereço de entrega e campo de observações.
- Apenas itens **disponíveis e com estoque** aparecem para seleção.
- Baixa automática de estoque ao confirmar o pedido; item fica indisponível se o estoque zerar.
- Cálculo automático do valor total do pedido.
- Seleção do método de pagamento (Dinheiro, Cartão de Crédito, Cartão de Débito ou Pix) com simulação de aprovação.
- Acompanhamento do pedido com status: **Aguardando → Preparando → Saiu para entrega → Entregue** (ou **Cancelado**).
- Cliente pode **cancelar** um pedido enquanto ele estiver "Aguardando" — o estoque dos itens é devolvido automaticamente.
- Administrador e Funcionário visualizam **todos** os pedidos; o Cliente vê **somente os seus**.
- Administrador e Funcionário podem avançar o status do pedido; apenas o Administrador pode cancelar um pedido em andamento.
- Bloqueio de alteração de status em pedidos já finalizados (Entregue/Cancelado).

### 📍 Endereços
- CRUD completo de endereços de entrega, vinculados ao usuário logado.
- Marcação de um endereço como "principal", com desmarcação automática dos demais.
- O primeiro endereço cadastrado é definido como principal automaticamente.
- Cada cliente só visualiza e edita os próprios endereços (filtro por `UsuarioId` em todas as consultas).

### ⭐ Avaliações
- Cliente pode avaliar (nota de 1 a 5 + comentário) **somente pedidos com status "Entregue"** e que pertençam a ele.
- Impede avaliação duplicada do mesmo pedido.
- Revalidação no `POST` do servidor (não confia apenas na tela de criação) para impedir que um usuário malicioso envie um formulário forjado avaliando pedido de terceiros.
- Listagem pública (para usuários logados) de todas as avaliações, com nome do avaliador e itens do pedido avaliado.
- Exclusão de avaliações — **restrita ao Administrador**.

### 👥 Gestão de usuários (Administrador)
- Listagem de todos os usuários cadastrados.
- Edição de nome, e-mail e perfil (Cliente ↔ Funcionário).
- Um Administrador **não pode** alterar o próprio perfil, e **ninguém** pode promover um usuário a Admin via formulário (mesmo manipulando o POST).
- Exclusão de usuários, com bloqueio explícito contra a auto-exclusão do Administrador pela tela de gestão.

### 🏠 Página inicial
- Dashboard de boas-vindas com horário de funcionamento, localização e contato da lanchonete.
- Atalho para "Fazer Pedido" (Cliente) ou "Gerenciar Usuários" (Admin), conforme o perfil logado.

---

## 🔑 Perfis de usuário e permissões

O sistema usa autorização baseada em **sessão** (sem cookies de autenticação do ASP.NET Identity), verificada manualmente em cada `Controller` através de métodos auxiliares (`Autenticado()`, `EhAdmin()`, `EhFuncionario()`, `EhCliente()`).

| Funcionalidade | Cliente | Funcionário | Administrador |
|---|---|---|---|
| Ver cardápio | ✅ | ✅ | ✅ |
| Criar / editar / excluir item do cardápio | ❌ | ❌ | ✅ |
| Atualizar estoque | ❌ | ❌ | ✅ |
| Registrar pedido | ✅ | ❌ | ❌ |
| Ver todos os pedidos | ❌ | ✅ | ✅ |
| Ver apenas os próprios pedidos | ✅ | — | — |
| Avançar status do pedido | ❌ | ✅ | ✅ |
| Cancelar pedido em andamento (já em preparo) | ❌ | ❌ | ✅ |
| Cancelar o próprio pedido ("Aguardando") | ✅ | — | — |
| Gerenciar endereços próprios | ✅ | ✅ | ✅ |
| Avaliar pedido entregue | ✅ | ❌ | ❌ |
| Excluir avaliação | ❌ | ❌ | ✅ |
| Gerenciar usuários (listar/editar/excluir) | ❌ | ❌ | ✅ |
| Promover usuário a Administrador | ❌ | ❌ | ❌ (bloqueado para todos) |

**Regras adicionais de segurança implementadas no servidor:**
- Toda verificação de permissão é repetida no `POST`, nunca confiando apenas na ocultação de botões na tela (defesa contra requisições forjadas).
- Um Funcionário não pode cancelar pedidos nem alterar pedidos já finalizados.
- Um Administrador não pode rebaixar/alterar o próprio perfil nem excluir a própria conta pela tela de usuários.
- Consultas a endereços e avaliações sempre filtram por `UsuarioId` da sessão, impedindo acesso cruzado entre contas.

---

## 🔄 Fluxo de um pedido

```
Cliente seleciona itens do cardápio (com estoque)
        │
        ▼
Escolhe endereço de entrega + observações
        │
        ▼
Pedido criado → status "Aguardando" + baixa de estoque
        │
        ▼
Seleciona forma de pagamento → pagamento simulado como "Aprovado"
        │
        ▼
   ┌─────────────────────────────────────────────┐
   │ Aguardando → Preparando → Saiu para entrega  │ → Entregue → (Cliente pode avaliar)
   └─────────────────────────────────────────────┘
        │
        ▼ (somente enquanto "Aguardando")
   Cancelado (estoque devolvido automaticamente)
```

---

## 🛠 Tecnologias utilizadas

| Camada | Tecnologia |
|---|---|
| Linguagem / Framework | C# / **ASP.NET Core MVC 6.0** |
| ORM | **Entity Framework Core 6.0** (Code-First + Migrations) |
| Banco de dados | **SQL Server** (LocalDB/SQLEXPRESS) |
| Views | Razor (`.cshtml`) |
| Front-end | HTML5, CSS3 (custom + Bootstrap), JavaScript |
| Sessão | `Microsoft.AspNetCore.Session` (estado de login em memória de servidor) |
| Padrão arquitetural | MVC (Model-View-Controller) |
| IDE utilizada | JetBrains Rider |

---

## 🏗 Arquitetura e estrutura do projeto

```
SaoJudasLanches/
└── SaoJudasLanches.Web/
    ├── Binders/
    │   └── DecimalModelBinder.cs        # Aceita preço com vírgula ou ponto (29,90 / 29.90)
    ├── Controllers/
    │   ├── AuthController.cs            # Login, cadastro, logout
    │   ├── AvaliacoesController.cs      # Avaliações de pedidos entregues
    │   ├── CardapioController.cs        # CRUD do cardápio + estoque
    │   ├── EnderecosController.cs       # CRUD de endereços do cliente
    │   ├── HomeController.cs            # Página inicial
    │   ├── PedidosController.cs         # Registro, pagamento, status, cancelamento
    │   └── UsuariosController.cs        # Gestão de usuários (Admin) + conta própria
    ├── Data/
    │   └── AppDbContext.cs              # DbContext do Entity Framework Core
    ├── Filters/
    │   └── NoCacheAttribute.cs          # Impede cache de páginas autenticadas
    ├── Migrations/                      # Histórico de migrations do EF Core
    ├── Models/
    │   ├── Usuario.cs
    │   ├── ItemCardapio.cs
    │   ├── Pedido.cs
    │   ├── ItemPedido.cs
    │   ├── Endereco.cs
    │   ├── Avaliacao.cs
    │   ├── AlterarSenhaViewModel.cs
    │   ├── PagamentoViewModel.cs
    │   └── RegistrarPedidoViewModel.cs
    ├── Views/
    │   ├── Auth/                        # Login, Cadastro
    │   ├── Avaliacoes/                  # Index, Criar
    │   ├── Cardapio/                    # Index, Criar, Editar
    │   ├── Enderecos/                   # Index, Criar, Editar
    │   ├── Home/                        # Index (dashboard)
    │   ├── Pedidos/                     # Index, Registrar, SelecionarPagamento, Acompanhar
    │   ├── Usuarios/                    # Index, Editar, AlterarSenha
    │   └── Shared/                      # _Layout, _AuthLayout
    ├── wwwroot/                         # CSS, JS e imagens estáticas
    ├── Program.cs                       # Configuração da aplicação (DI, pipeline, seed)
    ├── appsettings.json                 # Connection string e configurações
    └── SaoJudasLanches.Web.csproj
```

### Por que essa organização?
- **Controllers finos com checagem de sessão própria**: como o projeto não usa ASP.NET Identity, cada controller implementa métodos privados (`Autenticado`, `EhAdmin`, etc.) para centralizar a leitura do perfil armazenado na sessão, evitando duplicação de strings mágicas espalhadas pelas actions.
- **ViewModels dedicados** (`RegistrarPedidoViewModel`, `PagamentoViewModel`, `AlterarSenhaViewModel`) para telas que não mapeiam 1:1 com uma entidade do banco, mantendo as Models de domínio limpas.
- **Binder customizado de decimal**: resolve um problema comum em formulários brasileiros, onde o usuário digita preço com vírgula, mas o `decimal` nativo do .NET depende da cultura do servidor.
- **Filtro global `NoCacheAttribute`**: registrado uma única vez em `Program.cs` para todas as actions, evitando que o botão "voltar" do navegador exponha páginas autenticadas após logout.

---

## 🗄 Modelo de dados

**Usuario**
`Id (string/GUID)`, `Nome`, `Email`, `Senha`, `Perfil` (`Cliente` | `Funcionario` | `Admin`)

**ItemCardapio**
`Id`, `Nome`, `Descricao`, `Preco`, `Categoria`, `EstoqueAtual`, `Disponivel`

**Pedido**
`Id`, `UsuarioId` → Usuario, `EnderecoId` → Endereco, `DataPedido`, `Status`, `MetodoPagamento`, `StatusPagamento`, `Total`, `Observacoes`, `Itens` (1:N)

**ItemPedido**
`Id`, `PedidoId` → Pedido, `ItemCardapioId` → ItemCardapio, `Quantidade`, `PrecoUnitario`, `Subtotal` (calculado)

**Endereco**
`Id`, `UsuarioId` → Usuario, `Rua`, `Numero`, `Complemento`, `Bairro`, `Cidade`, `Estado`, `Cep`, `Principal`

**Avaliacao**
`Id`, `UsuarioId` → Usuario, `PedidoId` → Pedido, `Nota` (1–5), `Comentario`, `DataAvaliacao`

> No relacionamento `Avaliacao → Usuario` o `DeleteBehavior` foi explicitamente definido como `NoAction` em `AppDbContext.OnModelCreating`, evitando conflito de múltiplos caminhos de cascade delete que o SQL Server não permite (já que `Avaliacao` também referencia `Pedido`, que referencia `Usuario`).

---

## 💡 Decisões técnicas de destaque

- **Revalidação de autorização no servidor, sempre no `POST`**: nenhuma regra de permissão depende apenas de ocultar botões na View — todas são checadas novamente na action que recebe o formulário, prevenindo requisições forjadas (ex.: cliente tentando avaliar pedido de outra pessoa, ou Funcionário tentando cancelar pedido via chamada direta).
- **Estoque sempre consistente**: a baixa de estoque ocorre no momento do pedido e é **devolvida automaticamente** em caso de cancelamento, mantendo o número sempre fiel à realidade sem necessidade de jobs ou recálculos manuais.
- **Bloqueio de escalonamento de privilégio**: mesmo manipulando o corpo de um `POST`, não é possível promover um usuário a `Admin`, nem um Admin alterar o próprio perfil — essas regras estão no controller, não apenas na UI.
- **Model Binder customizado**: solução elegante para um problema real de formatação numérica em formulários PT-BR, sem precisar alterar a cultura global da aplicação.
- **Seed automático do Administrador**: ao subir a aplicação, o `Program.cs` verifica se existe ao menos um usuário; se não houver, cria o Admin padrão — facilita o setup do avaliador/professor sem passos manuais no banco.

---

## ▶️ Como executar o projeto

### Pré-requisitos
- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- SQL Server (LocalDB, Express ou completo)
- Visual Studio, Rider ou VS Code

### Passos

```bash
# 1. Clone o repositório
git clone <url-do-repositorio>
cd SaoJudasLanches/SaoJudasLanches.Web

# 2. Ajuste a connection string em appsettings.json, se necessário
# (por padrão usa "Server=localhost\\SQLEXPRESS")

# 3. Restaure os pacotes
dotnet restore

# 4. Aplique as migrations (cria o banco e as tabelas)
dotnet ef database update

# 5. Execute a aplicação
dotnet run
```

A aplicação abrirá em `https://localhost:7100` (ou a porta configurada em `Properties/launchSettings.json`). Na primeira execução, um usuário **Administrador** é criado automaticamente.

---

## 👤 Usuário administrador padrão

Criado automaticamente no primeiro startup da aplicação (caso a tabela de usuários esteja vazia):

| Campo | Valor |
|---|---|
| E-mail | `admin@saojudas.com` |
| Senha | `admin123` |
| Perfil | `Admin` |

> ⚠️ Recomenda-se alterar essa senha (ou removê-la do seed) antes de qualquer uso fora de ambiente de desenvolvimento/avaliação.

---

## ✅ Validações e regras de negócio

- Senha de cadastro com mínimo de 6 caracteres; e-mail não pode se repetir entre contas.
- Item de cardápio exige nome, preço entre R$ 0,01 e R$ 9.999,99 e categoria preenchida.
- Pedido só é criado se houver ao menos um item com quantidade maior que zero **e** estoque suficiente.
- Avaliação só é aceita para pedidos com status `Entregue` pertencentes ao próprio cliente, e apenas uma vez por pedido.
- Cancelamento de pedido só é permitido enquanto o status for `Aguardando`.
- Troca de senha exige confirmação da senha atual e confirmação da nova senha (com mínimo de 6 caracteres).

---

## ⚠️ Limitações conhecidas / próximos passos

- As senhas são armazenadas em **texto puro** no banco de dados — em uma evolução do projeto, o ideal seria aplicar hashing (ex.: BCrypt/Identity) antes de qualquer uso em produção.
- A autenticação é feita via sessão em memória do servidor (não há tokens/JWT), o que é adequado para o escopo acadêmico do projeto, mas não escala horizontalmente sem um provedor de sessão distribuído.
- O pagamento é **simulado** (sempre aprovado), sem integração real com gateways de pagamento.
- Não há testes automatizados (unitários/integração) no projeto até o momento.

---

## ✍️ Autor

Projeto desenvolvido por **Guilherme**, estudante de Análise e Desenvolvimento de Sistemas (FATEC São José do Rio Preto), como exercício prático de ASP.NET Core MVC, Entity Framework Core e modelagem de regras de negócio com múltiplos perfis de acesso.

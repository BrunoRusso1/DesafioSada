## Arquitetura

Optei por uma arquitetura em camadas simples, com o fluxo `Controller > Service > Repository`. O projeto é um CRUD pequeno, então não vi motivo para trazer CQRS, MediatR ou qualquer coisa mais pesada. A ideia foi manter o código fácil de ler, de testar e de evoluir.

## Como rodar

### Configuração (.env)

Os secret's ficam em um arquivo `.env` na raiz do projeto, que não vai para o git. Crie o seu a partir do exemplo:

```bash
cp .env.example .env
```

Depois ajuste os valores:

| Variável          | Descrição                                            |
|-------------------|------------------------------------------------------|
| `Jwt__Key`        | Chave de assinatura do JWT (mínimo de 32 caracteres) |
| `Admin__Username` | Usuário administrador                                |
| `Admin__Password` | Senha do administrador                               |

Sem esse arquivo a API não sobe, porque não há chave para assinar os tokens.

### Com Docker

Não precisa ter o .NET instalado:

```bash
docker build -t task-manager-api .
docker run --rm -p 8080:8080 --env-file .env task-manager-api
```

O Swagger fica em http://localhost:8080/swagger.

### Localmente

Com o .NET 8 SDK instalado:

```bash
dotnet run --project src/TaskManager.Api
```

O endereço aparece no console. Acrescente `/swagger` para abrir a documentação.

### Testes

```bash
dotnet test
```

## Autenticação

Os endpoints de tarefas só respondem com um token JWT válido. O login usa o administrador definido no `.env`.

Para pegar o token:

```bash
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"sua-senha"}'
```

Resposta (200):

```json
{
  "accessToken": "eyJhbGciOi...",
  "expiresAt": "2026-11-26T20:00:00Z"
}
```

Depois é só mandar o token no header `Authorization: Bearer {accessToken}` nas chamadas de tarefas. Sem token, ou com um token inválido ou expirado, a API devolve 401. Login com credenciais erradas também devolve 401.

No Swagger, execute o `POST /api/auth/login`, clique em **Authorize** e cole só o token, sem o prefixo `Bearer`.

## Endpoints

Todos os endpoints de `/api/tasks` exigem o token.

| Método | Rota              | O que faz                   | Respostas     |
|--------|-------------------|-----------------------------|---------------|
| POST   | `/api/auth/login` | Gera o token JWT            | 200, 400, 401 |
| GET    | `/api/tasks`      | Lista tarefas (com filtros) | 200           |
| GET    | `/api/tasks/{id}` | Busca uma tarefa pelo id    | 200, 404      |
| POST   | `/api/tasks`      | Cria uma tarefa             | 201, 400      |
| PUT    | `/api/tasks/{id}` | Atualiza uma tarefa         | 200, 400, 404 |
| DELETE | `/api/tasks/{id}` | Remove uma tarefa           | 204, 404      |

### Campos da tarefa

| Campo         | Regra                                                      |
|---------------|------------------------------------------------------------|
| `title`       | Obrigatório, até 100 caracteres                            |
| `description` | Opcional, até 500 caracteres                               |
| `dueDate`     | Opcional, formato ISO 8601 (ex.: `2026-12-01T10:00:00`)    |
| `status`      | `Pending`, `InProgress` ou `Completed` (padrão: `Pending`) |

### Filtros da listagem

Todos são opcionais e podem ser combinados:

- `status`: `Pending`, `InProgress` ou `Completed`
- `dueDate`: data de vencimento (ex.: `2026-12-01`)
- `title`: parte do título, para busca

### Exemplos

Criar uma tarefa:

```bash
curl -X POST http://localhost:8080/api/tasks \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"title":"Leitura","description":"Livro tal página tal","dueDate":"2026-12-01T10:00:00","status":"Pending"}'
```

Resposta (201), com o código único da tarefa em `id`:

```json
{
  "id": "6ae97bc7-7f2d-4213-8201-253229f68561",
  "title": "Leitura",
  "description": "Livro tal página tal",
  "dueDate": "2026-12-01T10:00:00",
  "status": "Pending"
}
```

Listar com filtros:

```bash
curl -H "Authorization: Bearer {token}" "http://localhost:8080/api/tasks?status=Pending&dueDate=2026-12-01"
```

Atualizar:

```bash
curl -X PUT http://localhost:8080/api/tasks/{id} \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"title":"Leitura","description":"Livro tal página tal","dueDate":"2026-12-01T10:00:00","status":"InProgress"}'
```

Excluir:

```bash
curl -X DELETE http://localhost:8080/api/tasks/{id} -H "Authorization: Bearer {token}"
```

## Tratamento de erros e logs

Um middleware global converte as exceções em respostas no formato `ProblemDetails`: 400 para dados inválidos, 401 para falta de autenticação, 404 para tarefa inexistente e 500 para falhas inesperadas, sem expor detalhes internos. Os logs usam o `ILogger` do .NET e saem no console, registrando criação, edição e exclusão de tarefas, logins (com sucesso ou não) e erros.

# CI/CD

O pipeline fica em [`.github/workflows/ci-cd.yml`](../.github/workflows/ci-cd.yml) e publica a imagem em **`ghcr.io/braynerdev/api-instartups-query`**.

## Gatilhos

| Evento | O que acontece |
| --- | --- |
| Pull request para `develop`, `homolog` ou `main` | Build e build da imagem **sem publicar**. |
| Push em `develop`, `homolog` ou `main` | Build e **publicação** da imagem. |
| Push de tag `v*.*.*` | Idem, com as tags SemVer. |
| Execução manual (`workflow_dispatch`) | Idem ao push, no branch escolhido. |

Mudanças só em `docs/**` ou arquivos `.md` não disparam o pipeline (exceto em tags).

## Jobs

1. **`build`**: `dotnet restore` → `dotnet build -c Release`. Ainda não há projetos de teste; quando houver, adicione o passo de `dotnet test` aqui, como na Command.
2. **`docker`** (depende de `build`): build de `infra/Dockerfile` para `linux/amd64`, com cache do GitHub Actions, push para o GHCR usando o `GITHUB_TOKEN` e atestado de proveniência da imagem.

## Tags da imagem

| Origem | Tags geradas |
| --- | --- |
| Push em `develop` | `develop`, `sha-<commit>` |
| Push em `homolog` | `homolog`, `sha-<commit>` |
| Push em `main` | `main`, `latest`, `sha-<commit>` |
| Tag `v1.2.3` | `1.2.3`, `1.2`, `sha-<commit>` |

## Publicando uma versão

Depois do merge na `main`:

```bash
git checkout main && git pull
git tag v1.0.0
git push origin v1.0.0
```

## Usando a imagem no servidor

O pacote nasce **privado**. Para puxar, faça login com um Personal Access Token (classic) com escopo `read:packages`:

```bash
echo "<token>" | docker login ghcr.io -u <usuario> --password-stdin
```

Ou torne o pacote público em *GitHub → Packages → api-instartups-query → Package settings*.

O `infra/docker-compose.yml` usa `ghcr.io/braynerdev/api-instartups-query:${IMAGE_TAG:-latest}` e **não builda** (é uma stack do Docker Swarm). No Portainer, cadastre o registry `ghcr.io` com o token acima, defina `IMAGE_TAG` nas variáveis da stack (`latest`, `homolog`, `develop`, `1.2.3` ou `sha-xxxxxxx`) e, a cada nova imagem, use *Update the stack* com **Re-pull image and redeploy**. Variáveis e chaves em [Configuração › Docker](CONFIGURACAO.md#docker); servidor (DNS, Traefik, certificados, Portainer) em `INSTARTUPS/infra/README.md`.

Para gerar a imagem localmente: `docker build -f infra/Dockerfile -t ghcr.io/braynerdev/api-instartups-query:local .` na raiz do repositório.

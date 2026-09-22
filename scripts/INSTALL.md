# DALBA - Deploy completo com banco quase vazio

Pacote de codigo-fonte para construir API .NET 9, Angular/Nginx e PostgreSQL 17.
Requer Docker Compose e internet para baixar imagens e dependencias. Nao e instalador offline.
O arquivo deploy/COMMIT.txt identifica a revisao publicada no GitHub.

## Conteudo do banco

deploy/database.sql contem schema e dados preparados e validados em banco temporario:
- Exatamente tres usuarios: admin, financeiro e fornecedor.
- Senhas iniciais: Admin@123, Financeiro@123 e Fornecedor@123, respectivamente.
- Configuracoes existentes em parametros_sistema da origem, mais valores padrao ausentes.
- Categorias e tipos de documento; um fornecedor minimo vinculado ao login fornecedor.
- Sem contratos, documentos enviados, NF, boletos, notificacoes ou auditoria antiga.

O banco de origem nao e apagado. O ZIP e o dump sao privados: podem conter chaves de integracao.
Nao enviar deploy/database.sql, .env ou backups ao GitHub. Trocar senhas iniciais no primeiro acesso.
Configuracoes de appsettings/.env nao sao exportadas como parametros: conferir integracoes apos instalar.

## Instalacao em ambiente novo

Extrair em uma pasta do servidor. Copiar .env.homologacao.example para .env e definir
POSTGRES_PASSWORD. O JWT_KEY ja e gerado aleatoriamente. Ajustar portas se necessario.

```powershell
Copy-Item .env.homologacao.example .env
docker compose -f docker-compose.yml -f compose.clean.yml up -d --build
docker compose -f docker-compose.yml -f compose.clean.yml ps
```

Abrir http://localhost:4200/login (ou dominio/IP do servidor e WEB_PORT).
Nginx encaminha /api para a API internamente, inclusive quando publicado com HTTPS.
O override compose.clean.yml carrega o dump automaticamente somente em volume NOVO
e desativa a criacao de dados de exemplo. Usar sempre os dois arquivos Compose.

## Servidor existente

Um volume existente NAO e limpo ao executar up. Nao remova volumes sem backup.
Para atualizar codigo mantendo dados e configuracoes do servidor, fazer backup primeiro
e reconstruir com os dois arquivos Compose. Nao restaurar o dump vazio nesse caso.
Se o volume existente usa PostgreSQL 16, manter essa imagem em um override proprio:
nao apontar PostgreSQL 17 diretamente para um volume de dados da versao 16.
Para substituir deliberadamente o banco por este banco quase vazio, instalar em volume
novo, com o ambiente anterior parado e preservado para retorno.

## Backup diario no Windows

O script salva database.dump, uploads.tar.gz e system.zip, incluindo .env e estrutura
de diretorios; gera hashes SHA256 e marcador SUCCESS. Retencao: 14 dias, apenas backups
completos. Falha interrompe a limpeza. O destino deve ficar fora da pasta do sistema.

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\backup-dalba.ps1 -ProjectDir "C:\Projetos\Dalba" -BackupDir "C:\Backups\Dalba"
powershell -ExecutionPolicy Bypass -File .\scripts\registrar-backup-diario.ps1 -ProjectDir "C:\Projetos\Dalba" -BackupDir "C:\Backups\Dalba" -Time "23:00"
```

Agendar no proprio servidor, usando uma conta com acesso ao Docker. A tarefa padrao
depende da sessao desse usuario e Docker Desktop em execucao. Para executar sem login,
configurar a identidade e credenciais no Agendador do Windows e garantir o Docker ativo.
Banco e uploads sao capturados em sequencia: para recuperacao consistente de arquivos,
usar janela sem uploads/alteracoes. Copiar backups tambem para armazenamento externo protegido.

## Restauracao de backup

Em ambiente de recuperacao, extrair system.zip preservando as pastas. Subir somente postgres
e restaurar o banco (sobrescreve os dados do destino):

```powershell
docker compose -f docker-compose.yml -f compose.clean.yml up -d postgres
docker cp database.dump dalba-postgres:/tmp/restore.dump
docker exec dalba-postgres pg_restore -U postgres -d DALBA --clean --if-exists --no-owner --no-privileges --exit-on-error /tmp/restore.dump
docker compose -f docker-compose.yml -f compose.clean.yml up -d --build api web
docker cp uploads.tar.gz dalba-api:/tmp/uploads.tar.gz
docker exec dalba-api tar -xzf /tmp/uploads.tar.gz -C /app/storage
```

Usar usuario/banco reais se diferentes de postgres/DALBA. Validar login, configuracoes,
consulta e abertura de documentos antes de liberar acesso. Restaurar em volume de uploads novo.

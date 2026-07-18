# Telas e Funcionalidades

## Login

- Autenticacao por login e senha.
- JWT armazenado no navegador.
- Recuperacao/redefinicao de senha.

## Dashboard

Disponivel para todos os perfis.

- Filtro por mes e ano de competencia.
- Admin: fornecedores, pendentes, enviados, em conformidade, contratos ativos e alertas.
- Fornecedor: situacao mensal, documentos faltantes, envios, notificacoes e NF pendente.
- Custos: conformidade, notas aguardadas, analise, liberados e pagos.

## Usuarios

Perfil: `Admin`.

- Criar, editar e excluir usuarios.
- Vincular fornecedor quando perfil for `Fornecedor`.

## Fornecedores

Perfis: `Admin`, `Financeiro`.

- Cadastro de PF/PJ.
- CPF/CNPJ com label dinamico.
- Porte apenas para pessoa juridica.
- Criacao automatica de usuario fornecedor.
- Importacao via Excel/CSV.

## Categorias

Perfil: `Admin`.

- Cadastro e manutencao de categorias.

## Contratos

Perfis: todos.

- Admin/Financeiro: incluir, editar e excluir.
- Fornecedor: apenas consultar contratos proprios.

## Documentos

Perfil: `Admin`.

- Cadastro do catalogo de tipos de documentos.

## Documentos Exigidos

Perfil: `Admin`.

- Define documentos obrigatorios por tipo de pessoa, porte e categoria.

## Envio Mensal

Perfil: `Fornecedor`.

- Abertura de competencia.
- Upload de um ou mais arquivos por documento.
- Reenvio apenas de documentos pendentes/reprovados.
- Documentos aprovados ficam bloqueados para alteracao.

## Validacao

Perfis: `Admin`, `Financeiro`.

- Filtro por competencia.
- Lista de fornecedores com envio completo.
- Analise documento por documento.
- Aprovar ou reprovar.
- Reprovacao remove o arquivo e volta o documento para pendente.

## Envio NF

Perfil: `Fornecedor`.

- Envio de nota fiscal PDF/XML.
- Campo AF obrigatorio.
- Checklist de validacao:
  - Numero da NF.
  - AF existe na NF.
  - Numero da AF igual ao da NF.
  - CNPJ/CPF confere.
  - Chave de acesso existe.
- Envio de boleto apos NF aceita.

## Custos

Perfis: `Admin`, `Financeiro`.

- Filtro por competencia.
- Lista de liberacoes.
- Atualizacao de status.
- Exportacao Excel.
- Exportacao PDF.

## Notificacoes

Perfis: todos.

- Historico de notificacoes por sistema, e-mail e SMS.

## Configuracao

Perfil: `Admin`.

- SMTP.
- SMS.
- API Key IA.
- API Key WhatsApp.

# Sistema

## Objetivo

Gerenciar o envio mensal de documentos por fornecedores, validacao administrativa, liberacao para envio de nota fiscal e acompanhamento por Custos.

## Perfis

- `Admin`: acesso aos cadastros, configuracoes, validacao e dashboards.
- `Financeiro`: acesso aos fornecedores, contratos, validacao documental e modulo Custos.
- `Fornecedor`: acesso aos seus contratos, envio mensal, envio de NF/boleto e notificacoes.

## Fluxo principal

1. Admin cadastra categorias, fornecedores, contratos e documentos exigidos.
2. Fornecedor abre a competencia mensal e envia documentos obrigatorios.
3. Admin ou Financeiro valida documento por documento.
4. Se algum documento for reprovado, o arquivo e removido e o fornecedor deve reenviar.
5. Se todos forem aprovados, a competencia fica em conformidade.
6. Fornecedor envia NF com validacao automatica.
7. Fornecedor envia boleto quando a NF esta aceita.
8. Custos acompanha status ate pagamento.

## Regras principais

- Fornecedor visualiza apenas seus dados.
- Contrato pertence a um fornecedor.
- Documento exigido depende de tipo de pessoa, porte e categoria.
- Pessoa fisica nao possui porte.
- Pessoa fisica usa CPF; pessoa juridica usa CNPJ.
- Documento aprovado nao deve ser alterado pelo fornecedor.
- Documento reprovado volta a pendente e deve ser reenviado.
- Envio de NF exige validacao de NF, AF, CNPJ/CPF e chave de acesso.

## Notificacoes

Eventos de envio, reprova, conformidade, NF e boleto sao registrados em `notificacoes` e preparados para envio por:

- E-mail.
- SMS.
- Sistema.

## Configuracoes

Admin pode configurar:

- SMTP.
- SMS.
- API Key de IA.
- API Key do WhatsApp.

Esses dados ficam em `parametros_sistema`.

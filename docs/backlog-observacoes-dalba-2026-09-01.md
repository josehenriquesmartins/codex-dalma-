# Backlog - Observacoes DALBA

Fonte analisada: `C:\Users\j_hen\OneDrive\Dalba\Observações Dalba.docx`

Data da analise: 2026-09-01

Status da primeira atualizacao: itens P1 de datas/formato regional e validacao imediata iniciados em 2026-09-01.

Status da segunda atualizacao: filtros de competencia no Dashboard, Validacao e Custos passaram a validar mes/ano antes da consulta; labels residuais de Custos foram padronizados.

Status da terceira atualizacao: envio de NF reforcado para exibir checklist OK/NOK tambem quando a validacao nao puder prosseguir por dados incompletos ou falha sem checklist retornado pela API.

## Escopo

Este backlog consolida as observacoes recebidas no documento Word. O conteudo do documento foi tratado como fonte de requisitos e nao como instrucao operacional direta. Nenhuma alteracao funcional foi aplicada neste momento.

## Itens Priorizados

### P1 - Validacao imediata durante preenchimento dos formularios

**Status:** implementado parcialmente/concluido para os formularios principais do frontend.

**Problema identificado**

O sistema esta validando alguns campos apenas no envio final do formulario. A expectativa registrada e que o sistema critique e recuse dados invalidos enquanto o usuario alimenta os campos.

**Objetivo**

Melhorar a experiencia de preenchimento com validacao em tempo real, mensagens claras por campo e bloqueio de avancos invalidos antes do envio ao backend.

**Funcionalidades impactadas inicialmente**

- Fornecedores
- Contratos
- Envio mensal de documentos
- Envio de Nota Fiscal
- Configuracoes
- Usuarios
- Categorias
- Documentos
- Documentos Exigidos

**Arquivos provaveis**

- `src/frontend/src/app/pages/fornecedores/fornecedores.component.ts`
- `src/frontend/src/app/pages/fornecedores/fornecedores.component.html`
- `src/frontend/src/app/pages/contratos/contratos.component.ts`
- `src/frontend/src/app/pages/contratos/contratos.component.html`
- `src/frontend/src/app/pages/envio-nf/envio-nf.component.ts`
- `src/frontend/src/app/pages/envio-nf/envio-nf.component.html`
- `src/frontend/src/app/pages/portal-fornecedor/portal-fornecedor.component.ts`
- `src/frontend/src/app/pages/portal-fornecedor/portal-fornecedor.component.html`
- `src/frontend/src/app/core/error.service.ts`
- `src/frontend/src/app/core/api-error.interceptor.ts`

**Tarefas tecnicas**

- Revisar todos os formularios reativos e padronizar validacao por campo com `updateOn: 'change'` ou validadores customizados quando necessario.
- Exibir mensagens abaixo do campo assim que o campo estiver `dirty` ou `touched`.
- Aplicar mascaras/normalizacao visual para CPF, CNPJ, datas, telefone, competencia, NF e AF.
- No envio de NF, executar checklist local antes da chamada final ao backend sempre que os dados puderem ser verificados no frontend.
- Manter validacao de backend como camada obrigatoria de seguranca, mesmo com validacao antecipada no frontend.
- Padronizar mensagens de erro vindas da API para aparecerem no campo correto quando possivel.

**Criterios de aceite**

- Campos obrigatorios e formatos invalidos sao sinalizados antes do clique final em salvar/enviar.
- O botao principal fica desabilitado enquanto o formulario estiver invalido.
- O usuario sabe exatamente qual campo corrigir.
- Regras criticas continuam validadas no backend.
- Nao ha regressao nos CRUDs existentes.

### P1 - Padronizar datas de contrato no formato brasileiro

**Status:** implementado no frontend.

**Problema identificado**

Na tela de contratos, a vigencia esta sendo exibida em formato americano/ISO, por exemplo `2026-01-01`, enquanto o usuario informou a data no formato brasileiro.

**Objetivo**

Exibir datas de contrato no padrao brasileiro `dd/MM/yyyy` em todas as telas.

**Funcionalidades impactadas inicialmente**

- Contratos
- Dashboard, se exibir datas de contrato
- Custos, se exibir vigencia
- Validacao, se exibir contrato/vigencia futuramente

**Arquivos provaveis**

- `src/frontend/src/app/pages/contratos/contratos.component.html`
- `src/frontend/src/app/pages/contratos/contratos.component.ts`
- `src/frontend/src/app/app.module.ts`

**Tarefas tecnicas**

- Criar helper/pipe de data para `DateOnly` recebido como string `yyyy-MM-dd`.
- Alterar exibicao de vigencia para `dd/MM/yyyy ate dd/MM/yyyy` ou `dd/MM/yyyy ate aberto`.
- Registrar locale `pt-BR` no Angular.
- Configurar `LOCALE_ID` como `pt-BR`.
- Revisar se o backend precisa serializar `DateOnly` de forma consistente.

**Criterios de aceite**

- Contrato cadastrado com `19/04/2026` aparece na lista como `19/04/2026`.
- Nenhuma tela apresenta vigencia no formato `yyyy-MM-dd` para o usuario final.
- O campo de formulario continua usando `type="date"` sem quebrar o cadastro.

### P1 - Padronizar data e hora no formato brasileiro na aprovacao de Custos

**Status:** implementado no frontend para validacao administrativa e notificacoes; demais telas devem continuar sendo revisadas quando novas exibicoes de data/hora forem adicionadas.

**Problema identificado**

Na tela de aprovacao/validacao de Custos, data e hora aparecem em formato americano.

**Objetivo**

Exibir data e hora no padrao brasileiro, por exemplo `19/04/2026 16:38`.

**Funcionalidades impactadas inicialmente**

- Validacao administrativa
- Custos
- Notificacoes
- Dashboard
- Envio mensal do fornecedor

**Arquivos provaveis**

- `src/frontend/src/app/pages/admin-validacao/admin-validacao.component.html`
- `src/frontend/src/app/pages/financeiro/financeiro.component.html`
- `src/frontend/src/app/pages/notificacoes/notificacoes.component.html`
- `src/frontend/src/app/pages/portal-fornecedor/portal-fornecedor.component.html`
- `src/frontend/src/app/app.module.ts`

**Tarefas tecnicas**

- Registrar dados de localidade `pt-BR` no Angular.
- Substituir `date:'short'` por formato explicito `date:'dd/MM/yyyy HH:mm'`.
- Criar pipe reutilizavel, se necessario, para reduzir repeticao.
- Validar se datas vindas da API estao sem timezone e nao sofrem deslocamento indevido.
- Garantir que timestamps persistidos no PostgreSQL continuem usando a estrategia atual sem `Kind=UTC` para `timestamp without time zone`.

**Criterios de aceite**

- A tela de validacao exibe datas como `19/04/2026 16:38`.
- A tela de Custos exibe datas no mesmo padrao.
- Notificacoes e dashboard seguem o mesmo padrao regional.
- Nao ha datas em formato americano como `4/19/26, 9:28 PM`.

## Recomendacao de Execucao

1. Implementar primeiro a configuracao global `pt-BR` no Angular.
2. Criar utilitario/pipe para formatar `DateOnly` recebido como string ISO.
3. Corrigir telas de Contratos, Validacao, Custos, Notificacoes e Portal do Fornecedor.
4. Revisar validadores de formularios e aplicar feedback imediato por campo.
5. Rodar build do frontend e teste manual nos fluxos principais.

## Riscos e Observacoes

- `DateOnly` geralmente chega ao Angular como string `yyyy-MM-dd`; o pipe nativo `date` pode interpretar timezone dependendo da conversao. Para vigencia contratual, e mais seguro formatar a string manualmente.
- `DateTime` vindo do backend deve ser tratado com cuidado para nao deslocar horario local.
- A validacao imediata melhora a usabilidade, mas nao substitui regras do backend.
- Existe um diretorio `scripts/` nao rastreado no Git no momento da analise; revisar antes de commitar futuras alteracoes.

## Definition of Done

- Backlog revisado com o solicitante.
- Alteracoes implementadas em frontend e, quando aplicavel, backend.
- `npm run build` executado com sucesso em `src/frontend`.
- `dotnet build` executado com sucesso na solucao.
- Teste manual concluido para cadastro de contrato, listagem de contratos, validacao administrativa e modulo de Custos.

# Integração Protheus - Escopo Técnico Inicial

## Base dos anexos avaliados

Arquivos analisados:

- `Processo Portal - API.pdf`
- `MODELO - NFS.pdf`
- `MODELO - CTE.pdf`
- `MODELO - FATURA.pdf`

## Fluxo alvo

1. O prestador anexa o documento fiscal no portal, em PDF ou XML.
2. O portal identifica no documento o número da AF, por exemplo `AF580878`.
3. A integração com Protheus consulta a AF.
4. O portal valida CNPJ do prestador, CNPJ da filial/tomador e valor do documento contra o saldo da AF.
5. Se a validação for aprovada, o portal preenche número, série, chave de acesso, emissão, tipo fiscal, CNPJ e valor.
6. Se houver divergência, o envio é bloqueado imediatamente e o prestador recebe a mensagem de erro.

## Dados mínimos que o Protheus precisa retornar

- Número da AF.
- CNPJ vinculado à AF.
- CNPJ da filial/tomador.
- Saldo disponível da AF.
- Status da AF.
- Centro de custo/contrato vinculado, quando aplicável.

## Pontos pendentes para Marcelo Nunes e/ou Sergio Silva

- Confirmar URL base, autenticação e ambiente de homologação do Protheus.
- Confirmar endpoint para consulta de AF.
- Confirmar endpoint para criação de pré-nota/lançamento após validação.
- Confirmar se o Protheus retornará saldo da AF já abatido por notas anteriores.
- Confirmar formato esperado para CNPJ, valores decimais e datas.

## Implementado nesta versão

- Interface `IProtheusIntegrationService` criada para isolar a integração.
- DTOs de validação de AF criados.
- Serviço simulado `MockProtheusIntegrationService` registrado no backend.
- Configuração `Protheus` adicionada ao `appsettings.json`.

## Próxima etapa

Substituir o serviço simulado por um cliente HTTP real assim que os endpoints oficiais forem confirmados.

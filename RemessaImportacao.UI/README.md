# Importação de Remessa – Windows Forms (VB.NET)

Este projeto implementa o fluxo de **importação manual de arquivos de remessa** conforme solicitado no desafio técnico de implantação de carteira.

A aplicação permite que o usuário selecione um arquivo `.xlsx`, valide sua estrutura, processe os dados de forma incremental e registre tanto os dados válidos quanto eventuais inconsistências encontradas durante o processamento.

---

## 📌 Objetivo

Disponibilizar uma funcionalidade que permita ao time operacional importar manualmente arquivos de remessa enviados diariamente por um banco parceiro, garantindo:

- Atualização correta da base de clientes e contratos  
- Registro e acompanhamento de inconsistências  
- Controle do processamento e rastreabilidade das importações  

---

## 🖥️ Tecnologias Utilizadas

- **VB.NET** (Windows Forms)
- **PostgreSQL** (banco de dados relacional)
- **Dapper** (micro ORM)
- **ExcelDataReader** (leitura de arquivos `.xlsx`)
- **HttpClient** + **RandomUser API** (integração externa)
- Arquitetura em camadas (UI / Application / Domain / Infrastructure)

---

## 🧱 Arquitetura

O projeto foi organizado em camadas para facilitar manutenção e separação de responsabilidades:

- **UI**  
  Interface Windows Forms responsável pela interação com o usuário.

- **Application**  
  Contém a orquestração do processo de importação e regras de negócio.

- **Domain**  
  DTOs e entidades que representam o domínio do problema.

- **Infrastructure**  
  Acesso a banco de dados, leitura de Excel, consumo de API externa e repositórios.

---

## 📂 Fluxo de Funcionamento

1. O usuário seleciona um arquivo `.xlsx` pela interface.
2. O sistema valida se o arquivo está aderente ao layout cadastrado no banco.
3. Caso haja divergência de layout:
   - O processamento é interrompido
   - O usuário é informado e pode optar por prosseguir ou cancelar
4. As linhas do arquivo são processadas individualmente:
   - Validações básicas de negócio são aplicadas
   - Clientes, contratos e parcelas são inseridos ou atualizados (processo incremental)
5. Inconsistências são registradas no banco e apresentadas ao usuário ao final.
6. A importação é finalizada com status e totais consolidados.

---

## ✅ Regras de Negócio Implementadas

- CPF/CNPJ obrigatório (normalizado apenas com números)
- Contrato obrigatório
- Número da parcela maior que zero
- Vencimento válido
- Valor da parcela maior que zero
- Importação incremental (update ou insert conforme existência do registro)

---

## ⚠️ Tratamento de Inconsistências

- Cada linha com erro é tratada individualmente
- O processamento continua mesmo com inconsistências
- As falhas são:
  - Registradas no banco de dados
  - Apresentadas ao usuário em um pop-up ao final da execução

---

## 🌐 Integração Externa

Para cada cliente, o sistema consulta a API pública: https://randomuser.me/api/


Os dados de endereço retornados são utilizados para complementar o cadastro do cliente.  
Em caso de falha na API, é aplicado um endereço de fallback para garantir a continuidade do processo.

---

## 🗄️ Banco de Dados

O banco utilizado é **PostgreSQL**, com tabelas separadas para:

- Importação
- Inconsistências
- Clientes
- Contratos
- Parcelas
- Layout do arquivo

O modelo segue o conceito de **espelhamento da carteira**, conforme solicitado.

---

## ▶️ Execução do Projeto

1. Abrir a solução no Visual Studio
2. Configurar a string de conexão no `App.config`
3. Executar a aplicação (F5)
4. Selecionar o arquivo `.xlsx` de remessa
5. Iniciar o processamento pela interface

---

## 📌 Observações Finais

O projeto foi desenvolvido priorizando:
- Clareza de código
- Separação de responsabilidades
- Tratamento seguro de erros
- Facilidade de entendimento para manutenção futura

---

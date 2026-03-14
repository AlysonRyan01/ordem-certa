# OrdemCerta --- SaaS de Gestão de Ordens de Serviço

## Visão Geral

O **OrdemCerta** é um sistema SaaS (Software as a Service) voltado para
**assistências técnicas de eletrônicos**, como:

- TVs
- celulares
- eletrodomésticos
- computadores
- outros equipamentos eletrônicos

O objetivo da plataforma é **organizar o fluxo completo de atendimento
técnico**, desde o cadastro do cliente até a entrega do equipamento,
incluindo **envio e aprovação de orçamentos via WhatsApp**.

Cada empresa possui um **ambiente isolado**, contendo seus próprios
clientes, ordens de serviço e funcionários.

---

# Modelo de Planos

## Plano Demo

Plano gratuito para teste do sistema.

Limitações:

- até **10 clientes cadastrados**
- até **10 ordens de serviço**
- acesso às funcionalidades principais

Quando o limite é atingido, o sistema solicita **upgrade para o plano
pago**.

---

## Plano Pago

Plano completo do sistema.

Inclui:

- clientes ilimitados
- ordens de serviço ilimitadas
- múltiplos funcionários
- integração com WhatsApp
- todas as funcionalidades da plataforma

---

# Estrutura Multiempresa (Multi‑tenant)

Cada empresa cadastrada possui:

- dados da empresa
- lista de clientes
- ordens de serviço
- funcionários

Os dados são **isolados entre empresas**, garantindo segurança e
privacidade.

---

# Cadastro de Clientes

Cada cliente pode possuir informações como:

- nome
- telefone (WhatsApp)
- CPF/CNPJ
- endereço
- observações

Um cliente pode possuir **várias ordens de serviço ao longo do tempo**.

---

# Ordens de Serviço

A ordem de serviço representa um equipamento deixado para análise ou
conserto.

## Informações do Equipamento

- tipo de aparelho (TV, celular, microondas, etc)
- marca
- modelo
- defeito relatado
- acessórios entregues (ex: controle remoto, carregador)
- observações

## Informações Administrativas

- data de entrada
- técnico responsável
- status da ordem
- orçamento

---

# Status da Ordem de Serviço

Exemplos de status possíveis:

- Recebido
- Em análise
- Orçamento pendente
- Aguardando aprovação
- Orçamento aprovado
- Orçamento recusado
- Em conserto
- Pronto para retirada
- Entregue
- Cancelado

---

# Fluxo de Orçamento via WhatsApp

Uma das principais funcionalidades do sistema é a **aprovação de
orçamento pelo cliente diretamente pelo WhatsApp**.

## 1. Criação do orçamento

O funcionário da eletrônica adiciona:

- valor do orçamento
- descrição do serviço
- observações

O status da ordem muda para:

**Aguardando aprovação**

---

## 2. Envio automático via WhatsApp

Após salvar o orçamento, o sistema envia automaticamente uma mensagem
para o cliente contendo:

- identificação da eletrônica
- resumo da ordem
- valor do orçamento
- link para visualizar a ordem

Também é enviado **o número de contato da eletrônica** para dúvidas.

---

## 3. Página pública da ordem

Ao clicar no link recebido no WhatsApp, o cliente acessa uma página
contendo:

- dados do equipamento
- problema relatado
- valor do orçamento
- descrição do serviço

E dois botões:

- Aprovar orçamento
- Não aprovar orçamento

---

## 4. Resposta do cliente

Quando o cliente seleciona uma opção, o frontend envia uma requisição
para a API contendo:

- identificador da ordem
- decisão do cliente

---

## 5. Atualização da ordem

Se aprovado:

Status → **Orçamento aprovado**

Se recusado:

Status → **Orçamento recusado**

---

## 6. Notificação para a eletrônica

Após a resposta do cliente, o sistema envia uma notificação informando:

- aprovação ou recusa
- identificação da ordem

---

# Painel Administrativo da Empresa

## Dashboard

O painel apresenta indicadores como:

- ordens abertas
- ordens aguardando aprovação
- ordens em conserto
- ordens prontas para retirada
- faturamento estimado

---

## Listagem de Ordens

Filtros disponíveis:

- status
- cliente
- data
- técnico responsável

---

## Histórico da Ordem

Cada ordem mantém registro de eventos como:

- criação
- envio de orçamento
- aprovação ou recusa
- mudança de status
- entrega do equipamento

---

# Página Pública de Acompanhamento

Opcionalmente o cliente pode acompanhar o andamento da ordem por uma
página pública.

Isso reduz perguntas frequentes como:

"Meu aparelho já está pronto?"

---

# Benefícios para a Eletrônica

O sistema resolve diversos problemas operacionais:

- organização das ordens de serviço
- comunicação automatizada com clientes
- aprovação rápida de orçamentos
- histórico completo de atendimento
- redução de ligações e mensagens manuais

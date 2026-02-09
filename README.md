# ChatBot.Net (OmniChat)

Este repositório já possui uma **base importante** para um sistema de chatbot omnichannel (com foco em WhatsApp), mas ainda está em estágio de evolução.

Abaixo está um diagnóstico objetivo do que está:
- ✅ **Pronto / implementado**
- 🟡 **Parcial / protótipo funcional**
- ❌ **Faltando para produção**

---

## 1) Visão geral do que existe hoje

### Estrutura de projetos
- `OmniChat.API`: API HTTP + Webhook + SignalR.
- `OmniChat.Application`: casos de uso e orquestradores (IA, fluxo, híbrido).
- `OmniChat.Domain`: entidades, interfaces, regras de domínio e VO de criptografia.
- `OmniChat.Infrastructure`: integração com MongoDB, canais (WhatsApp/Telegram), IA (OpenAI/Gemini), autenticação.
- `OmniChat.Client`: painel web (Blazor + MudBlazor) com login, cadastro, fluxo, chat ao vivo e admin.
- `OmniChat.WebUI`: UI secundária (páginas exemplo/monitor).
- `OmniChat.Shared`: DTOs compartilhados.

---

## 2) Status funcional por área

## ✅ Pronto / já implementado

### Backend base
- API com controllers de autenticação (`/api/auth/login` e `/api/auth/register`).
- Endpoint de verificação do webhook da Meta (`GET /api/webhook/meta`).
- Endpoint de recebimento de mensagens da Meta (`POST /api/webhook/meta`).
- Hub SignalR (`/chathub`) para atualização em tempo real no painel.

### Segurança e conta
- Hash de senha com BCrypt.
- Geração de JWT com claims de usuário/role/organização.
- Criptografia de mensagens (VO `EncryptedText`) no lado de domínio.

### Persistência e domínio
- Modelagem de planos, features, assinatura e organização multi-tenant.
- Contexto MongoDB com coleções de usuários, planos, fluxos, organizações e sessões MCP.
- Seed inicial de plano gratuito no startup da API.

### Integrações externas (base)
- Canal de envio para WhatsApp Cloud API.
- Canal de envio para Telegram.
- Serviços de IA para OpenAI e Gemini.
- Fábrica de IA com fallback entre provedores.

### Front-end (painel)
- Telas de cadastro/login.
- Layout com áreas de tenant/admin.
- Tela de fluxo visual (editor com árvore, seleção de nó e opções).
- Tela de live chat com conexão SignalR e troca de mensagens básica.

---

## 🟡 Parcial / em estágio de protótipo

### Orquestração chatbot
- Existe orquestrador seguro com:
  - checagem de plano,
  - carregamento de contexto MCP,
  - criptografia de entrada/saída,
  - chamada de IA com fallback.
- Existe orquestrador híbrido (Fluxo + IA + handover humano) e engine de fluxo.
- **Porém**, o encadeamento completo ainda depende de ajustes de DI e integração entre camadas para funcionar ponta a ponta em produção.

### Live chat e monitoramento
- SignalR está presente e UI recebe mensagens.
- `ChatHub.SendMessageToCustomer` ainda está sem implementação real de envio.
- Parte das telas usa dados mockados/estáticos (ex.: dashboard e histórico inicial de chat).

### Cadastro e onboarding
- Cadastro cria organização + admin + vínculo de plano.
- Ainda faltam validações de robustez (ex.: conflito de email/telefone, regras anti-duplicidade mais amplas, fluxos de erro e observabilidade).

### Base de conhecimento e contexto
- Existe repositório de busca vetorial (`KnowledgeRepository`) para MongoDB Atlas Vector Search.
- Ainda não está totalmente conectado ao pipeline principal de resposta para funcionar como RAG completo na prática.

---

## ❌ Faltando para ficar pronto para produção

### 1. Infra de aplicação e DI
- Registrar todos os serviços no `Program.cs` da API (orquestradores, canais, serviços de IA, `HttpClient`, `IdempotencyService`, implementações de interfaces, etc.).
- Implementar e registrar `IMcpRepository` (hoje existe interface, mas falta implementação concreta no repositório).

### 2. Segurança e compliance
- Remover segredos hardcoded/defaults e adotar Secret Manager/Key Vault.
- Endurecer JWT (issuer, audience, rotação de segredo, políticas completas).
- Revisar criptografia do cliente (há implementação com chave/IV fixos para demo).
- Implementar trilhas de auditoria e mascaramento de dados sensíveis.

### 3. WhatsApp produção
- Validar assinatura do webhook (assinatura Meta/X-Hub-Signature quando aplicável).
- Tratar todos os tipos de payload (status, mídia, eventos, erros de entrega).
- Política de retry/backoff + DLQ/filas para alta confiabilidade.
- Normalização E.164 e gestão de múltiplos canais por usuário.

### 4. Fluxos, handover e atendimento humano
- Persistir e recuperar histórico real do chat para operador.
- Implementar roteamento completo de handover (fila, agente, encerramento, retomada pelo bot).
- Finalizar CRUD real de fluxos no backend (hoje o editor é majoritariamente visual/protótipo).

### 5. Qualidade e operação
- Criar suíte de testes (unitário, integração, contrato, webhook, carga).
- Adicionar logs estruturados, métricas e tracing (OpenTelemetry).
- Pipeline CI/CD com lint, build, testes e validações de segurança.
- Estratégia de migração/versionamento de dados e backup/restore.

### 6. Governança de planos e billing
- Fechar regras de limites por plano (mensagens/campanhas/usuários por período).
- Integrar billing/assinatura (provedor de pagamento, renovação, inadimplência, downgrade).

---

## 3) Checklist objetivo (resumo executivo)

## Funcionalidades de um chatbot WhatsApp
- [x] Receber mensagens via webhook.
- [x] Responder mensagens via WhatsApp API.
- [x] Login/cadastro com JWT e organização.
- [x] Estrutura para IA multi-provedor (OpenAI/Gemini).
- [x] Estrutura para fluxo conversacional.
- [x] Estrutura para atendimento humano em tempo real.
- [ ] Operação robusta de produção (resiliência, filas, observabilidade).
- [ ] Segurança/compliance completos para ambiente real.
- [ ] Testes automatizados e CI/CD maduros.
- [ ] Implementação completa de MCP repository + wiring total de DI.

---

## 4) Próximos passos recomendados (ordem sugerida)

1. **Fechar infraestrutura mínima executável**
   - Implementar `IMcpRepository` em Mongo.
   - Corrigir/expandir injeção de dependências no `OmniChat.API/Program.cs`.

2. **Fechar fluxo WhatsApp ponta a ponta**
   - Garantir idempotência + validação de assinatura + tratamento de erros de API Meta.
   - Persistência e recuperação do histórico real no painel.

3. **Hardening de segurança**
   - Secret manager e revisão de criptografia cliente/servidor.
   - Políticas de auth e autorização por tenant/role.

4. **Confiabilidade operacional**
   - Fila para processamento de webhook.
   - Observabilidade completa (logs/métricas/traces/alertas).

5. **Qualidade e deploy**
   - Testes automatizados.
   - CI/CD com gates de qualidade.

---

## 5) Conclusão

O projeto está com uma **fundação técnica boa** (arquitetura em camadas, domínio de planos, webhook WhatsApp, IA multi-provedor, SignalR, painel Blazor), mas ainda é **MVP avançado / protótipo funcional** e **não está pronto para produção crítica** sem os itens de robustez, segurança e operação listados acima.

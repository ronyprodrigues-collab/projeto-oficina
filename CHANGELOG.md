# Changelog

Todas as mudanças notáveis deste projeto serão documentadas aqui.

## [0.4.0] - 2025-11-14
- Módulo de estoque FIFO completo: catálogo de peças, movimentações de entrada/saída, integração com ordens de serviço e migração inicial única para facilitar instalações limpas.
- OS x Estoque: modal de peças busca o cadastro e usa o preço de venda; ao aprovar/editar/excluir ou reprovar uma OS o estoque é debitado ou devolvido automaticamente, mantendo histórico de referência ao lote.
- Aprovação e perfis: mecânicos só visualizam/adotam OS já aprovadas; supervisores/admin recebem cartão com OS pendentes de aprovação e ações de Aprovar/Reprovar diretamente nos detalhes.
- Correção: devoluções registram o mesmo custo unitário consumido na saída (sem usar o preço de venda), mantendo o histórico financeiro fiel.
- Detalhes de OS: destaque visual quando o cliente já aprovou, com botão para reprovar; badge no layout e totais mantidos conforme novas regras.
- Reprovação: motivo obrigatório, OS reprovada não pode mais ser editada/concluída e aparece sinalizada na lista para consulta.
- SaaS: suporte cria grupos definindo o plano (Básico/Pro/Plus) e agora configura cores primária/secundária exclusivas por grupo. Os formulários de Grupos ganharam pickers de cor, o índice mostra a paleta e, ao selecionar uma oficina, o layout aplica automaticamente as cores do grupo escolhido.
- Configurações por oficina: tela do Suporte permite escolher grupo e oficina para editar logo, cores, nome e plano individual de cada oficina (com prévia de tema). O layout usa o plano e as cores da oficina atual, caindo no padrão do grupo/configuração apenas quando não houver personalização local.

## [0.5.0] - 2025-11-17
- Multi-oficinas: todos os perfis (Admin, Supervisor, Mecânico e Diretor) são redirecionados para a tela de seleção após o login, escolhendo explicitamente o grupo e a oficina antes de acessar qualquer módulo. O contexto é limpo ao trocar de usuário.
- Compartilhamento de cadastros: Clientes e Veículos podem ser reaproveitados entre oficinas do mesmo grupo. As telas ganharam botón “Vincular existente” e listam a origem de cada item, além de novas páginas para buscar e vincular registros globais.
- Configurações por oficina: Suporte agora seleciona grupo e oficina em um dashboard unificado para trocar plano, logo e cores de maneira independente. As cores escolhidas são aplicadas no layout assim que a oficina é selecionada.
- Modelo/migrações: campos de identidade visual foram adicionados às entidades `GrupoOficina` e `Oficina`, com seeds atualizados e migrations `GrupoCores`/`OficinaVisual`. O layout passou a consumir essas informações diretamente do contexto da oficina.
- Documentação: changelog atualizado com as evoluções SaaS e ajustes de plano/cores.

## [0.3.0] - 2025-11-12
- Painel: filtros por mês, mecânico e cliente para o gráfico e faturamento do mês.
- Ordens de Serviço: detalhes com visão do mecânico (sem valores), conclusão parcial de Serviços/Peças e conclusão automática da OS ao finalizar todos os itens.
- Segurança/UI: mecânico redireciona para "Minhas OS"; lista geral restrita a Admin/Supervisor.
- Funcionários (Admin): CRUD básico para Supervisores e Mecânicos (criar, editar cargo/senha, desativar/reativar).
- Tema: 6 presets prontos com aplicação global (navbar, botões, links, tabelas/DataTables, paginação e inputs) + contraste aprimorado.
- Locale e valores: binding pt-BR estável; normalização de casas decimais; migração com precisão para decimais.
- Dados de demonstração: geração automática de 10 clientes, veículos e 50 OS (com serviços/peças) em bancos vazios.
- Primeiro acesso: suporte para definir Admin via env vars/appsettings (InitialAdmin:Email/Password/Name) ao iniciar.

## [0.2.0] - 2025-11-12
- Ordens de Serviço: unificação das telas de Create/Edit/Details no mesmo layout com modais de Serviços e Peças e totalizadores dinâmicos.
- Mecânico: dropdown agora lista somente usuários com a role "Mecanico" e exibe o `NomeCompleto`.
- Painel: cartão "Pendentes de aprovação" conta OS não aprovadas e não concluídas; faturamento mensal permanece real com base em OS concluídas.
- Melhorias de UX: formatação de valores com 2 casas decimais; recálculo robusto; carregamento de itens existentes no Edit.

## [0.1.0] - 2025-11-12
- Base inicial com MVC, Identity (roles e seed), EF Core e CRUDs de Clientes/Veículos/OS, layout, tema e DataTables.

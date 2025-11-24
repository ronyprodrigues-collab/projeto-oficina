CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;
ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `AspNetRoles` (
    `Id` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `Name` varchar(256) CHARACTER SET utf8mb4 NULL,
    `NormalizedName` varchar(256) CHARACTER SET utf8mb4 NULL,
    `ConcurrencyStamp` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_AspNetRoles` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `AspNetUsers` (
    `Id` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `NomeCompleto` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Cargo` longtext CHARACTER SET utf8mb4 NOT NULL,
    `UserName` varchar(256) CHARACTER SET utf8mb4 NULL,
    `NormalizedUserName` varchar(256) CHARACTER SET utf8mb4 NULL,
    `Email` varchar(256) CHARACTER SET utf8mb4 NULL,
    `NormalizedEmail` varchar(256) CHARACTER SET utf8mb4 NULL,
    `EmailConfirmed` bit(1) NOT NULL,
    `PasswordHash` longtext CHARACTER SET utf8mb4 NULL,
    `SecurityStamp` longtext CHARACTER SET utf8mb4 NULL,
    `ConcurrencyStamp` longtext CHARACTER SET utf8mb4 NULL,
    `PhoneNumber` longtext CHARACTER SET utf8mb4 NULL,
    `PhoneNumberConfirmed` bit(1) NOT NULL,
    `TwoFactorEnabled` bit(1) NOT NULL,
    `LockoutEnd` datetime(6) NULL,
    `LockoutEnabled` bit(1) NOT NULL,
    `AccessFailedCount` int NOT NULL,
    CONSTRAINT `PK_AspNetUsers` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Clientes` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Nome` longtext CHARACTER SET utf8mb4 NOT NULL,
    `CPF_CNPJ` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Telefone` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Email` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Endereco` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Numero` longtext CHARACTER SET utf8mb4 NULL,
    `Bairro` longtext CHARACTER SET utf8mb4 NULL,
    `Cidade` longtext CHARACTER SET utf8mb4 NULL,
    `Estado` longtext CHARACTER SET utf8mb4 NULL,
    `CEP` longtext CHARACTER SET utf8mb4 NULL,
    `DataNascimento` datetime(6) NULL,
    `Observacoes` longtext CHARACTER SET utf8mb4 NULL,
    `TipoCliente` longtext CHARACTER SET utf8mb4 NOT NULL,
    `CNPJ` longtext CHARACTER SET utf8mb4 NULL,
    `Responsavel` longtext CHARACTER SET utf8mb4 NULL,
    `IsDeleted` bit(1) NOT NULL,
    `DeletedAt` datetime(6) NULL,
    CONSTRAINT `PK_Clientes` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Configuracoes` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `LogoPath` longtext CHARACTER SET utf8mb4 NULL,
    `CorPrimaria` longtext CHARACTER SET utf8mb4 NOT NULL,
    `CorSecundaria` longtext CHARACTER SET utf8mb4 NOT NULL,
    `NomeOficina` longtext CHARACTER SET utf8mb4 NOT NULL,
    `PlanoAtual` int NOT NULL,
    `IsDeleted` bit(1) NOT NULL,
    `DeletedAt` datetime(6) NULL,
    CONSTRAINT `PK_Configuracoes` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `AspNetRoleClaims` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `RoleId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `ClaimType` longtext CHARACTER SET utf8mb4 NULL,
    `ClaimValue` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_AspNetRoleClaims` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_AspNetRoleClaims_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `AspNetUserClaims` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `ClaimType` longtext CHARACTER SET utf8mb4 NULL,
    `ClaimValue` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_AspNetUserClaims` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_AspNetUserClaims_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `AspNetUserLogins` (
    `LoginProvider` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `ProviderKey` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `ProviderDisplayName` longtext CHARACTER SET utf8mb4 NULL,
    `UserId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_AspNetUserLogins` PRIMARY KEY (`LoginProvider`, `ProviderKey`),
    CONSTRAINT `FK_AspNetUserLogins_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `AspNetUserRoles` (
    `UserId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `RoleId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_AspNetUserRoles` PRIMARY KEY (`UserId`, `RoleId`),
    CONSTRAINT `FK_AspNetUserRoles_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_AspNetUserRoles_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `AspNetUserTokens` (
    `UserId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `LoginProvider` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `Name` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `Value` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_AspNetUserTokens` PRIMARY KEY (`UserId`, `LoginProvider`, `Name`),
    CONSTRAINT `FK_AspNetUserTokens_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `Grupos` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Nome` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `Descricao` longtext CHARACTER SET utf8mb4 NULL,
    `DiretorId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `Plano` int NOT NULL,
    `CorPrimaria` longtext CHARACTER SET utf8mb4 NOT NULL,
    `CorSecundaria` longtext CHARACTER SET utf8mb4 NOT NULL,
    `IsDeleted` bit(1) NOT NULL,
    `DeletedAt` datetime(6) NULL,
    CONSTRAINT `PK_Grupos` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Grupos_AspNetUsers_DiretorId` FOREIGN KEY (`DiretorId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `Veiculos` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Placa` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Marca` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Modelo` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Ano` int NOT NULL,
    `ClienteId` int NOT NULL,
    `IsDeleted` bit(1) NOT NULL,
    `DeletedAt` datetime(6) NULL,
    CONSTRAINT `PK_Veiculos` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Veiculos_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `Oficinas` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Nome` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `Descricao` longtext CHARACTER SET utf8mb4 NULL,
    `GrupoOficinaId` int NOT NULL,
    `Plano` int NOT NULL,
    `CorPrimaria` longtext CHARACTER SET utf8mb4 NOT NULL,
    `CorSecundaria` longtext CHARACTER SET utf8mb4 NOT NULL,
    `LogoPath` longtext CHARACTER SET utf8mb4 NULL,
    `AdminProprietarioId` varchar(255) CHARACTER SET utf8mb4 NULL,
    `IsDeleted` bit(1) NOT NULL,
    `DeletedAt` datetime(6) NULL,
    CONSTRAINT `PK_Oficinas` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Oficinas_AspNetUsers_AdminProprietarioId` FOREIGN KEY (`AdminProprietarioId`) REFERENCES `AspNetUsers` (`Id`),
    CONSTRAINT `FK_Oficinas_Grupos_GrupoOficinaId` FOREIGN KEY (`GrupoOficinaId`) REFERENCES `Grupos` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `CategoriasFinanceiras` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `OficinaId` int NOT NULL,
    `Nome` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Tipo` int NOT NULL,
    `Descricao` longtext CHARACTER SET utf8mb4 NULL,
    `Ativo` bit(1) NOT NULL,
    `IsDeleted` bit(1) NOT NULL,
    `DeletedAt` datetime(6) NULL,
    CONSTRAINT `PK_CategoriasFinanceiras` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_CategoriasFinanceiras_Oficinas_OficinaId` FOREIGN KEY (`OficinaId`) REFERENCES `Oficinas` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `ContasFinanceiras` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `OficinaId` int NOT NULL,
    `Nome` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Tipo` int NOT NULL,
    `SaldoInicial` decimal(18,2) NOT NULL,
    `Banco` longtext CHARACTER SET utf8mb4 NULL,
    `Agencia` longtext CHARACTER SET utf8mb4 NULL,
    `NumeroConta` longtext CHARACTER SET utf8mb4 NULL,
    `Ativo` bit(1) NOT NULL,
    `IsDeleted` bit(1) NOT NULL,
    `DeletedAt` datetime(6) NULL,
    CONSTRAINT `PK_ContasFinanceiras` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ContasFinanceiras_Oficinas_OficinaId` FOREIGN KEY (`OficinaId`) REFERENCES `Oficinas` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `OficinasClientes` (
    `OficinaId` int NOT NULL,
    `ClienteId` int NOT NULL,
    `VinculadoEm` datetime(6) NOT NULL,
    `Observacao` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_OficinasClientes` PRIMARY KEY (`OficinaId`, `ClienteId`),
    CONSTRAINT `FK_OficinasClientes_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_OficinasClientes_Oficinas_OficinaId` FOREIGN KEY (`OficinaId`) REFERENCES `Oficinas` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `OficinasUsuarios` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `OficinaId` int NOT NULL,
    `UsuarioId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `Perfil` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Ativo` bit(1) NOT NULL,
    `VinculadoEm` datetime(6) NOT NULL,
    CONSTRAINT `PK_OficinasUsuarios` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_OficinasUsuarios_AspNetUsers_UsuarioId` FOREIGN KEY (`UsuarioId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_OficinasUsuarios_Oficinas_OficinaId` FOREIGN KEY (`OficinaId`) REFERENCES `Oficinas` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `OficinasVeiculos` (
    `OficinaId` int NOT NULL,
    `VeiculoId` int NOT NULL,
    `VinculadoEm` datetime(6) NOT NULL,
    `Observacao` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_OficinasVeiculos` PRIMARY KEY (`OficinaId`, `VeiculoId`),
    CONSTRAINT `FK_OficinasVeiculos_Oficinas_OficinaId` FOREIGN KEY (`OficinaId`) REFERENCES `Oficinas` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_OficinasVeiculos_Veiculos_VeiculoId` FOREIGN KEY (`VeiculoId`) REFERENCES `Veiculos` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `OrdensServico` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ClienteId` int NOT NULL,
    `VeiculoId` int NOT NULL,
    `MecanicoId` varchar(255) CHARACTER SET utf8mb4 NULL,
    `OficinaId` int NOT NULL,
    `Descricao` longtext CHARACTER SET utf8mb4 NOT NULL,
    `DataAbertura` datetime(6) NOT NULL,
    `DataPrevista` datetime(6) NULL,
    `DataConclusao` datetime(6) NULL,
    `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
    `AprovadaCliente` bit(1) NOT NULL,
    `MotivoReprovacao` longtext CHARACTER SET utf8mb4 NULL,
    `EstoqueReservado` bit(1) NOT NULL,
    `Observacoes` longtext CHARACTER SET utf8mb4 NULL,
    `IsDeleted` bit(1) NOT NULL,
    `DeletedAt` datetime(6) NULL,
    CONSTRAINT `PK_OrdensServico` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_OrdensServico_AspNetUsers_MecanicoId` FOREIGN KEY (`MecanicoId`) REFERENCES `AspNetUsers` (`Id`),
    CONSTRAINT `FK_OrdensServico_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_OrdensServico_Oficinas_OficinaId` FOREIGN KEY (`OficinaId`) REFERENCES `Oficinas` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_OrdensServico_Veiculos_VeiculoId` FOREIGN KEY (`VeiculoId`) REFERENCES `Veiculos` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `PecaEstoques` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Nome` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
    `Codigo` varchar(40) CHARACTER SET utf8mb4 NULL,
    `UnidadeMedida` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
    `EstoqueMinimo` decimal(18,4) NOT NULL,
    `SaldoAtual` decimal(18,4) NOT NULL,
    `PrecoVenda` decimal(18,2) NULL,
    `OficinaId` int NOT NULL,
    `IsDeleted` bit(1) NOT NULL,
    `DeletedAt` datetime(6) NULL,
    CONSTRAINT `PK_PecaEstoques` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_PecaEstoques_Oficinas_OficinaId` FOREIGN KEY (`OficinaId`) REFERENCES `Oficinas` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `LancamentosFinanceiros` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `OficinaId` int NOT NULL,
    `Tipo` int NOT NULL,
    `CategoriaFinanceiraId` int NOT NULL,
    `ContaPadraoId` int NULL,
    `ClienteId` int NULL,
    `ParceiroNome` longtext CHARACTER SET utf8mb4 NULL,
    `Descricao` longtext CHARACTER SET utf8mb4 NOT NULL,
    `NumeroDocumento` longtext CHARACTER SET utf8mb4 NULL,
    `DataCompetencia` datetime(6) NOT NULL,
    `ValorTotal` decimal(18,2) NOT NULL,
    `Origem` longtext CHARACTER SET utf8mb4 NULL,
    `Observacao` longtext CHARACTER SET utf8mb4 NULL,
    `IsDeleted` bit(1) NOT NULL,
    `DeletedAt` datetime(6) NULL,
    CONSTRAINT `PK_LancamentosFinanceiros` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_LancamentosFinanceiros_CategoriasFinanceiras_CategoriaFinanc~` FOREIGN KEY (`CategoriaFinanceiraId`) REFERENCES `CategoriasFinanceiras` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_LancamentosFinanceiros_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_LancamentosFinanceiros_ContasFinanceiras_ContaPadraoId` FOREIGN KEY (`ContaPadraoId`) REFERENCES `ContasFinanceiras` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_LancamentosFinanceiros_Oficinas_OficinaId` FOREIGN KEY (`OficinaId`) REFERENCES `Oficinas` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `ServicoItem` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Descricao` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Valor` decimal(18,2) NOT NULL,
    `Concluido` bit(1) NOT NULL,
    `OrdemServicoId` int NOT NULL,
    `IsDeleted` bit(1) NOT NULL,
    `DeletedAt` datetime(6) NULL,
    CONSTRAINT `PK_ServicoItem` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ServicoItem_OrdensServico_OrdemServicoId` FOREIGN KEY (`OrdemServicoId`) REFERENCES `OrdensServico` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `MovimentacoesEstoque` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `PecaEstoqueId` int NOT NULL,
    `DataMovimentacao` datetime(6) NOT NULL,
    `Tipo` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `Quantidade` decimal(18,4) NOT NULL,
    `ValorUnitario` decimal(18,4) NOT NULL,
    `QuantidadeRestante` decimal(18,4) NOT NULL,
    `Observacao` varchar(200) CHARACTER SET utf8mb4 NULL,
    `OrdemServicoId` int NULL,
    `OficinaId` int NOT NULL,
    `MovimentacaoEntradaReferenciaId` int NULL,
    `IsDeleted` bit(1) NOT NULL,
    `DeletedAt` datetime(6) NULL,
    CONSTRAINT `PK_MovimentacoesEstoque` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_MovimentacoesEstoque_MovimentacoesEstoque_MovimentacaoEntrad~` FOREIGN KEY (`MovimentacaoEntradaReferenciaId`) REFERENCES `MovimentacoesEstoque` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_MovimentacoesEstoque_Oficinas_OficinaId` FOREIGN KEY (`OficinaId`) REFERENCES `Oficinas` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_MovimentacoesEstoque_PecaEstoques_PecaEstoqueId` FOREIGN KEY (`PecaEstoqueId`) REFERENCES `PecaEstoques` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `PecaItem` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Nome` longtext CHARACTER SET utf8mb4 NOT NULL,
    `ValorUnitario` decimal(18,2) NOT NULL,
    `Quantidade` int NOT NULL,
    `Concluido` bit(1) NOT NULL,
    `PecaEstoqueId` int NULL,
    `OrdemServicoId` int NOT NULL,
    `IsDeleted` bit(1) NOT NULL,
    `DeletedAt` datetime(6) NULL,
    CONSTRAINT `PK_PecaItem` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_PecaItem_OrdensServico_OrdemServicoId` FOREIGN KEY (`OrdemServicoId`) REFERENCES `OrdensServico` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_PecaItem_PecaEstoques_PecaEstoqueId` FOREIGN KEY (`PecaEstoqueId`) REFERENCES `PecaEstoques` (`Id`) ON DELETE SET NULL
) CHARACTER SET=utf8mb4;

CREATE TABLE `LancamentoParcelas` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `LancamentoFinanceiroId` int NOT NULL,
    `Numero` int NOT NULL,
    `DataVencimento` datetime(6) NOT NULL,
    `Valor` decimal(18,2) NOT NULL,
    `Situacao` int NOT NULL,
    `DataPagamento` datetime(6) NULL,
    `ContaPagamentoId` int NULL,
    `Observacao` longtext CHARACTER SET utf8mb4 NULL,
    `IsDeleted` bit(1) NOT NULL,
    `DeletedAt` datetime(6) NULL,
    CONSTRAINT `PK_LancamentoParcelas` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_LancamentoParcelas_ContasFinanceiras_ContaPagamentoId` FOREIGN KEY (`ContaPagamentoId`) REFERENCES `ContasFinanceiras` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_LancamentoParcelas_LancamentosFinanceiros_LancamentoFinancei~` FOREIGN KEY (`LancamentoFinanceiroId`) REFERENCES `LancamentosFinanceiros` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_AspNetRoleClaims_RoleId` ON `AspNetRoleClaims` (`RoleId`);

CREATE UNIQUE INDEX `RoleNameIndex` ON `AspNetRoles` (`NormalizedName`);

CREATE INDEX `IX_AspNetUserClaims_UserId` ON `AspNetUserClaims` (`UserId`);

CREATE INDEX `IX_AspNetUserLogins_UserId` ON `AspNetUserLogins` (`UserId`);

CREATE INDEX `IX_AspNetUserRoles_RoleId` ON `AspNetUserRoles` (`RoleId`);

CREATE INDEX `EmailIndex` ON `AspNetUsers` (`NormalizedEmail`);

CREATE UNIQUE INDEX `UserNameIndex` ON `AspNetUsers` (`NormalizedUserName`);

CREATE INDEX `IX_CategoriasFinanceiras_OficinaId` ON `CategoriasFinanceiras` (`OficinaId`);

CREATE INDEX `IX_ContasFinanceiras_OficinaId` ON `ContasFinanceiras` (`OficinaId`);

CREATE INDEX `IX_Grupos_DiretorId` ON `Grupos` (`DiretorId`);

CREATE UNIQUE INDEX `IX_Grupos_Nome` ON `Grupos` (`Nome`);

CREATE INDEX `IX_LancamentoParcelas_ContaPagamentoId` ON `LancamentoParcelas` (`ContaPagamentoId`);

CREATE INDEX `IX_LancamentoParcelas_LancamentoFinanceiroId` ON `LancamentoParcelas` (`LancamentoFinanceiroId`);

CREATE INDEX `IX_LancamentosFinanceiros_CategoriaFinanceiraId` ON `LancamentosFinanceiros` (`CategoriaFinanceiraId`);

CREATE INDEX `IX_LancamentosFinanceiros_ClienteId` ON `LancamentosFinanceiros` (`ClienteId`);

CREATE INDEX `IX_LancamentosFinanceiros_ContaPadraoId` ON `LancamentosFinanceiros` (`ContaPadraoId`);

CREATE INDEX `IX_LancamentosFinanceiros_OficinaId` ON `LancamentosFinanceiros` (`OficinaId`);

CREATE INDEX `IX_MovimentacoesEstoque_MovimentacaoEntradaReferenciaId` ON `MovimentacoesEstoque` (`MovimentacaoEntradaReferenciaId`);

CREATE INDEX `IX_MovimentacoesEstoque_OficinaId` ON `MovimentacoesEstoque` (`OficinaId`);

CREATE INDEX `IX_MovimentacoesEstoque_PecaEstoqueId` ON `MovimentacoesEstoque` (`PecaEstoqueId`);

CREATE INDEX `IX_Oficinas_AdminProprietarioId` ON `Oficinas` (`AdminProprietarioId`);

CREATE UNIQUE INDEX `IX_Oficinas_GrupoOficinaId_Nome` ON `Oficinas` (`GrupoOficinaId`, `Nome`);

CREATE INDEX `IX_OficinasClientes_ClienteId` ON `OficinasClientes` (`ClienteId`);

CREATE UNIQUE INDEX `IX_OficinasUsuarios_OficinaId_UsuarioId` ON `OficinasUsuarios` (`OficinaId`, `UsuarioId`);

CREATE INDEX `IX_OficinasUsuarios_UsuarioId` ON `OficinasUsuarios` (`UsuarioId`);

CREATE INDEX `IX_OficinasVeiculos_VeiculoId` ON `OficinasVeiculos` (`VeiculoId`);

CREATE INDEX `IX_OrdensServico_ClienteId` ON `OrdensServico` (`ClienteId`);

CREATE INDEX `IX_OrdensServico_MecanicoId` ON `OrdensServico` (`MecanicoId`);

CREATE INDEX `IX_OrdensServico_OficinaId` ON `OrdensServico` (`OficinaId`);

CREATE INDEX `IX_OrdensServico_VeiculoId` ON `OrdensServico` (`VeiculoId`);

CREATE INDEX `IX_PecaEstoques_OficinaId` ON `PecaEstoques` (`OficinaId`);

CREATE INDEX `IX_PecaItem_OrdemServicoId` ON `PecaItem` (`OrdemServicoId`);

CREATE INDEX `IX_PecaItem_PecaEstoqueId` ON `PecaItem` (`PecaEstoqueId`);

CREATE INDEX `IX_ServicoItem_OrdemServicoId` ON `ServicoItem` (`OrdemServicoId`);

CREATE INDEX `IX_Veiculos_ClienteId` ON `Veiculos` (`ClienteId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20251118145900_InitialMySql', '9.0.11');

COMMIT;


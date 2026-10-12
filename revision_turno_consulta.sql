START TRANSACTION;
ALTER TABLE `Consultas` ADD `IdTurno` int NULL;

CREATE UNIQUE INDEX `IX_Consultas_IdTurno` ON `Consultas` (`IdTurno`);

ALTER TABLE `Consultas` ADD CONSTRAINT `FK_Consultas_Turnos_IdTurno` FOREIGN KEY (`IdTurno`) REFERENCES `Turnos` (`IdTurno`) ON DELETE RESTRICT;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261012011033_RelacionOpcionalTurnoConsulta', '9.0.0');

COMMIT;


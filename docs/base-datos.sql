CREATE DATABASE  IF NOT EXISTS `veterinaria` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `veterinaria`;
-- MySQL dump 10.13  Distrib 8.0.45, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: veterinaria
-- ------------------------------------------------------
-- Server version	8.0.45

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `__efmigrationshistory`
--

DROP TABLE IF EXISTS `__efmigrationshistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `__efmigrationshistory` (
  `MigrationId` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProductVersion` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `__efmigrationshistory`
--

LOCK TABLES `__efmigrationshistory` WRITE;
/*!40000 ALTER TABLE `__efmigrationshistory` DISABLE KEYS */;
INSERT INTO `__efmigrationshistory` VALUES ('20260930151121_Inicial','9.0.0'),('20260930155005_Inicial','9.0.0'),('20260930205923_AgregarEspecieAMascota','9.0.0'),('20261009145941_TratamientoComoAtributo','9.0.0'),('20261012011033_RelacionOpcionalTurnoConsulta','9.0.0'),('20261012151022_CrearTablaVeterinarios','9.0.0'),('20261012154626_RelacionVeterinarioConsulta','9.0.0'),('20261020143740_RelacionUsuarioVeterinario','9.0.0');
/*!40000 ALTER TABLE `__efmigrationshistory` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `clientes`
--

DROP TABLE IF EXISTS `clientes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `clientes` (
  `IdCliente` int NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Apellido` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Dni` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Telefono` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Correo` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Direccion` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Activo` tinyint(1) NOT NULL,
  PRIMARY KEY (`IdCliente`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `clientes`
--

LOCK TABLES `clientes` WRITE;
/*!40000 ALTER TABLE `clientes` DISABLE KEYS */;
INSERT INTO `clientes` VALUES (1,' Juan ',' Pérez','32145678','2664123456','juan.perez@gmail.com','Av. España 1255',1),(2,'María','González','35478216',' 2664987654','maria.gonzalez@gmail.com','Belgrano 450',1),(3,' Carlos','Rodríguez','29874561','2664234567','carlos.rodriguez@gmail.com',' Rivadavia 820',1),(4,'Lucía ','Fernández','40123654',' 2664765432','lucia.fernandez@gmail.com','Mitre 315',1),(5,'Diego','Martínez ','33789124',' 2664456789','diego.martinez@gmail.com','Pringles 1020',1);
/*!40000 ALTER TABLE `clientes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `consultas`
--

DROP TABLE IF EXISTS `consultas`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `consultas` (
  `IdConsulta` int NOT NULL AUTO_INCREMENT,
  `FechaHora` datetime(6) NOT NULL,
  `Motivo` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Diagnostico` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Observaciones` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `IdMascota` int NOT NULL,
  `IdUsuario` int NOT NULL,
  `Tratamiento` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `IdTurno` int DEFAULT NULL,
  `IdVeterinario` int DEFAULT NULL,
  PRIMARY KEY (`IdConsulta`),
  UNIQUE KEY `IX_Consultas_IdTurno` (`IdTurno`),
  KEY `IX_Consultas_IdMascota` (`IdMascota`),
  KEY `IX_Consultas_IdUsuario` (`IdUsuario`),
  KEY `IX_Consultas_IdVeterinario` (`IdVeterinario`),
  CONSTRAINT `FK_Consultas_Mascotas_IdMascota` FOREIGN KEY (`IdMascota`) REFERENCES `mascotas` (`IdMascota`) ON DELETE CASCADE,
  CONSTRAINT `FK_Consultas_Turnos_IdTurno` FOREIGN KEY (`IdTurno`) REFERENCES `turnos` (`IdTurno`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Consultas_Usuarios_IdUsuario` FOREIGN KEY (`IdUsuario`) REFERENCES `usuarios` (`IdUsuario`) ON DELETE CASCADE,
  CONSTRAINT `FK_Consultas_Veterinarios_IdVeterinario` FOREIGN KEY (`IdVeterinario`) REFERENCES `veterinarios` (`IdVeterinario`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `consultas`
--

LOCK TABLES `consultas` WRITE;
/*!40000 ALTER TABLE `consultas` DISABLE KEYS */;
INSERT INTO `consultas` VALUES (5,'2026-10-20 12:20:00.000000','lastimada de una pata','corte','proximo control en 2 semanas',7,3,'pomada',6,3),(6,'2026-10-20 12:27:00.000000','control mensual','no hay',NULL,2,3,'---',8,3);
/*!40000 ALTER TABLE `consultas` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `especies`
--

DROP TABLE IF EXISTS `especies`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `especies` (
  `IdEspecie` int NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Activo` tinyint(1) NOT NULL,
  PRIMARY KEY (`IdEspecie`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `especies`
--

LOCK TABLES `especies` WRITE;
/*!40000 ALTER TABLE `especies` DISABLE KEYS */;
INSERT INTO `especies` VALUES (1,' Perro',1),(2,' Gato',1),(3,'Ave',1),(4,'Conejo',1),(5,'Reptil',1);
/*!40000 ALTER TABLE `especies` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `historiasclinicas`
--

DROP TABLE IF EXISTS `historiasclinicas`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `historiasclinicas` (
  `IdHClinica` int NOT NULL AUTO_INCREMENT,
  `NombreArchivo` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CarpetaDescarga` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `FormatoArchivo` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `CargaArchivo` datetime(6) NOT NULL,
  `Descripcion` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `IdConsulta` int NOT NULL,
  PRIMARY KEY (`IdHClinica`),
  KEY `IX_HistoriasClinicas_IdConsulta` (`IdConsulta`),
  CONSTRAINT `FK_HistoriasClinicas_Consultas_IdConsulta` FOREIGN KEY (`IdConsulta`) REFERENCES `consultas` (`IdConsulta`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `historiasclinicas`
--

LOCK TABLES `historiasclinicas` WRITE;
/*!40000 ALTER TABLE `historiasclinicas` DISABLE KEYS */;
INSERT INTO `historiasclinicas` VALUES (2,'How to introduce yourself in English.pdf','e4cbc9d9bb89477ab9f939baebff9b11.pdf','.pdf','2026-10-20 12:33:52.457825',NULL,5),(3,'WhatsApp Image 2026-10-20 at 12.34.09.jpeg','d64d3d86e762473fb0604163e09af94b.jpeg','.jpeg','2026-10-20 12:34:33.777811',NULL,5),(4,'WhatsApp Image 2026-10-20 at 21.55.03.jpeg','814b9be009ae4982834f5792b7d7285a.jpeg','.jpeg','2026-10-21 22:48:37.342394',NULL,5);
/*!40000 ALTER TABLE `historiasclinicas` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mascotas`
--

DROP TABLE IF EXISTS `mascotas`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `mascotas` (
  `IdMascota` int NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `FechaNacimiento` datetime(6) DEFAULT NULL,
  `Sexo` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Peso` decimal(65,30) DEFAULT NULL,
  `Observaciones` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Activo` tinyint(1) NOT NULL,
  `IdCliente` int NOT NULL,
  `IdRaza` int NOT NULL,
  `IdEspecie` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`IdMascota`),
  KEY `IX_Mascotas_IdCliente` (`IdCliente`),
  KEY `IX_Mascotas_IdRaza` (`IdRaza`),
  KEY `IX_Mascotas_IdEspecie` (`IdEspecie`),
  CONSTRAINT `FK_Mascotas_Clientes_IdCliente` FOREIGN KEY (`IdCliente`) REFERENCES `clientes` (`IdCliente`) ON DELETE CASCADE,
  CONSTRAINT `FK_Mascotas_Especies_IdEspecie` FOREIGN KEY (`IdEspecie`) REFERENCES `especies` (`IdEspecie`) ON DELETE CASCADE,
  CONSTRAINT `FK_Mascotas_Razas_IdRaza` FOREIGN KEY (`IdRaza`) REFERENCES `razas` (`IdRaza`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mascotas`
--

LOCK TABLES `mascotas` WRITE;
/*!40000 ALTER TABLE `mascotas` DISABLE KEYS */;
INSERT INTO `mascotas` VALUES (1,'Luna','2022-04-15 00:00:00.000000','Hembra',24.500000000000000000000000000000,'Vacunación al día',1,1,1,1),(2,'Toby','2021-08-10 00:00:00.000000','Macho',6.200000000000000000000000000000,'Control anual',1,2,3,1),(3,'Rocky','2020-01-22 00:00:00.000000','Macho',31.800000000000000000000000000000,'Requiere control de peso',1,3,4,1),(4,'Mishi','2023-06-03 00:00:00.000000','Hembra',4.100000000000000000000000000000,'Sin observaciones',1,4,7,2),(5,'Simba','2022-11-18 00:00:00.000000','Macho',4.800000000000000000000000000000,'Control de pelaje',1,5,8,2),(6,'Coco','2024-02-05 00:00:00.000000','Macho',0.030000000000000000000000000000,'Control general',1,1,9,3),(7,'Nala','2023-09-12 00:00:00.000000','Hembra',2.100000000000000000000000000000,' Alimentación especial',1,2,12,4),(8,'Tito','2021-03-20 00:00:00.000000','Macho',3.500000000000000000000000000000,'Control de alimentación',1,3,13,5),(9,'Juancho','2023-05-22 00:00:00.000000','Macho',2.100000000000000000000000000000,'Control Gral',1,4,13,5);
/*!40000 ALTER TABLE `mascotas` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `razas`
--

DROP TABLE IF EXISTS `razas`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `razas` (
  `IdRaza` int NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Activo` tinyint(1) NOT NULL,
  `IdEspecie` int NOT NULL,
  PRIMARY KEY (`IdRaza`),
  KEY `IX_Razas_IdEspecie` (`IdEspecie`),
  CONSTRAINT `FK_Razas_Especies_IdEspecie` FOREIGN KEY (`IdEspecie`) REFERENCES `especies` (`IdEspecie`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=15 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `razas`
--

LOCK TABLES `razas` WRITE;
/*!40000 ALTER TABLE `razas` DISABLE KEYS */;
INSERT INTO `razas` VALUES (1,' Labrador ',1,1),(3,'Caniche',1,1),(4,'Pastor Alemán',1,1),(5,'Golden Retriever',1,1),(6,'Bulldog Francés',1,1),(7,'Siamés',1,2),(8,'Persa',1,2),(9,'Canario',1,3),(10,'Periquito',1,3),(11,'Belier',1,4),(12,'Rex',1,4),(13,'Iguana',1,5),(14,'Tortuga de tierra',1,5);
/*!40000 ALTER TABLE `razas` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `turnos`
--

DROP TABLE IF EXISTS `turnos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `turnos` (
  `IdTurno` int NOT NULL AUTO_INCREMENT,
  `FechaHora` datetime(6) NOT NULL,
  `Motivo` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Estado` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Observaciones` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `IdMascota` int NOT NULL,
  `IdUsuario` int NOT NULL,
  PRIMARY KEY (`IdTurno`),
  KEY `IX_Turnos_IdMascota` (`IdMascota`),
  KEY `IX_Turnos_IdUsuario` (`IdUsuario`),
  CONSTRAINT `FK_Turnos_Mascotas_IdMascota` FOREIGN KEY (`IdMascota`) REFERENCES `mascotas` (`IdMascota`) ON DELETE CASCADE,
  CONSTRAINT `FK_Turnos_Usuarios_IdUsuario` FOREIGN KEY (`IdUsuario`) REFERENCES `usuarios` (`IdUsuario`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `turnos`
--

LOCK TABLES `turnos` WRITE;
/*!40000 ALTER TABLE `turnos` DISABLE KEYS */;
INSERT INTO `turnos` VALUES (1,'2026-10-09 19:30:00.000000','Co','Confirmado',NULL,2,1),(3,'2026-10-09 19:29:00.000000','ff','Confirmado',NULL,1,1),(4,'2026-10-16 19:45:00.000000','Consulta','Confirmado',NULL,3,1),(5,'2026-10-22 21:00:00.000000','Control general','Pendiente',NULL,8,1),(6,'2026-10-14 09:00:00.000000','lastimada de una pata','Atendido',NULL,7,1),(7,'2026-10-21 16:00:00.000000','antiparasitario','Pendiente',NULL,5,1),(8,'2026-10-30 09:00:00.000000','control mensual','Atendido',NULL,2,2),(9,'0001-01-13 12:00:00.000000','control','Pendiente',NULL,6,3);
/*!40000 ALTER TABLE `turnos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `usuarios`
--

DROP TABLE IF EXISTS `usuarios`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usuarios` (
  `IdUsuario` int NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Email` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Clave` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Rol` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Avatar` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Activo` tinyint(1) NOT NULL,
  PRIMARY KEY (`IdUsuario`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usuarios`
--

LOCK TABLES `usuarios` WRITE;
/*!40000 ALTER TABLE `usuarios` DISABLE KEYS */;
INSERT INTO `usuarios` VALUES (1,'Natalia','nc@gmail.com','AQAAAAIAAYagAAAAECVS+s5AyZdoARvSeP58/vDZbWN8rwFwfU3kqP08INNFu1JuJjOv+pCUfhXW1lWRCw==','Administrador','/uploads/usuarios/usuario_1_66dd163652924affa8e7968226428910.png',1),(2,'Guillermo','gc@gmail.com','AQAAAAIAAYagAAAAEGt83WBTEi7ZuO7Ao0K5sa1tEeLkKptl8G/1v8Clq1FX/WgwEJYtafKjRBG4FaeQww==','Empleado',NULL,1),(3,'Joselopez','jl@gmail.com','AQAAAAIAAYagAAAAECxKLvRxJpG7PK31PTikX03q/Lv5MQAGVQLfILiPHS5hmVXOWapvxTEvAI1iFunX+w==','Veterinario',NULL,1);
/*!40000 ALTER TABLE `usuarios` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `veterinarios`
--

DROP TABLE IF EXISTS `veterinarios`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `veterinarios` (
  `IdVeterinario` int NOT NULL AUTO_INCREMENT,
  `Matricula` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Especialidad` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Telefono` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `IdUsuario` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`IdVeterinario`),
  UNIQUE KEY `IX_Veterinarios_Matricula` (`Matricula`),
  UNIQUE KEY `IX_Veterinarios_IdUsuario` (`IdUsuario`),
  CONSTRAINT `FK_Veterinarios_Usuarios_IdUsuario` FOREIGN KEY (`IdUsuario`) REFERENCES `usuarios` (`IdUsuario`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `veterinarios`
--

LOCK TABLES `veterinarios` WRITE;
/*!40000 ALTER TABLE `veterinarios` DISABLE KEYS */;
INSERT INTO `veterinarios` VALUES (3,'890MM','perros','56433332',3);
/*!40000 ALTER TABLE `veterinarios` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-10-26 13:18:36

/*M!999999\- enable the sandbox mode */ 
-- MariaDB dump 10.19  Distrib 10.6.21-MariaDB, for Win64 (AMD64)
--
-- Host: localhost    Database: ipanalyze
-- ------------------------------------------------------
-- Server version	10.6.21-MariaDB

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `city_tb`
--

DROP TABLE IF EXISTS `city_tb`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `city_tb` (
  `PID` int(11) NOT NULL AUTO_INCREMENT,
  `NAME` varchar(50) NOT NULL COMMENT '(시/군/구) 명칭',
  `CREATE_DT` datetime NOT NULL DEFAULT current_timestamp() COMMENT '생성일',
  `UPDATE_DT` datetime DEFAULT NULL COMMENT '수정일',
  `DEL_YN` tinyint(1) DEFAULT 0 COMMENT '삭제유무',
  `DELETE_DT` datetime DEFAULT NULL COMMENT '삭제일',
  `COUNTRYTB_ID` int(11) NOT NULL COMMENT '(도/시) 테이블 키',
  PRIMARY KEY (`PID`),
  UNIQUE KEY `UK` (`NAME`),
  KEY `fk_countrytb202502191016` (`COUNTRYTB_ID`),
  CONSTRAINT `fk_countrytb202502191016` FOREIGN KEY (`COUNTRYTB_ID`) REFERENCES `country_tb` (`PID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='(시/군/구) 정보';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `city_tb`
--

LOCK TABLES `city_tb` WRITE;
/*!40000 ALTER TABLE `city_tb` DISABLE KEYS */;
INSERT INTO `city_tb` VALUES (1,'하남시','2025-02-21 12:57:49',NULL,0,NULL,1),(2,'사상구','2025-02-21 13:00:13',NULL,0,NULL,3),(3,'북구','2025-02-21 13:01:09',NULL,0,NULL,3),(4,'강남구','2025-02-21 13:01:50',NULL,0,NULL,4);
/*!40000 ALTER TABLE `city_tb` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `country_tb`
--

DROP TABLE IF EXISTS `country_tb`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `country_tb` (
  `PID` int(11) NOT NULL AUTO_INCREMENT,
  `NAME` varchar(50) NOT NULL COMMENT '(도/시)명칭',
  `CREATE_DT` datetime NOT NULL DEFAULT current_timestamp() COMMENT '생성일',
  `UPDATE_DT` datetime DEFAULT NULL COMMENT '수정일',
  `DEL_YN` tinyint(1) DEFAULT 0 COMMENT '삭제유무',
  `DELETE_DT` datetime DEFAULT NULL COMMENT '삭제일',
  PRIMARY KEY (`PID`),
  UNIQUE KEY `UK` (`NAME`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='(도/시) 정보';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `country_tb`
--

LOCK TABLES `country_tb` WRITE;
/*!40000 ALTER TABLE `country_tb` DISABLE KEYS */;
INSERT INTO `country_tb` VALUES (1,'경기도','2025-02-21 12:57:49',NULL,0,NULL),(3,'부산광역시','2025-02-21 13:00:13',NULL,0,NULL),(4,'서울시','2025-02-21 13:01:50',NULL,0,NULL);
/*!40000 ALTER TABLE `country_tb` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `login_tb`
--

DROP TABLE IF EXISTS `login_tb`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `login_tb` (
  `PID` int(11) NOT NULL AUTO_INCREMENT COMMENT 'PROCESS_ID',
  `UID` varchar(25) NOT NULL COMMENT '사용자ID',
  `PWD` varchar(25) NOT NULL COMMENT '비밀번호',
  `MASTER_YN` tinyint(1) NOT NULL DEFAULT 0 COMMENT '마스터계정 유무',
  `ADMIN_YN` tinyint(1) NOT NULL DEFAULT 0 COMMENT '관리자계정 유무',
  `USE_YN` tinyint(1) NOT NULL DEFAULT 0 COMMENT '로그인 승인 유무',
  `CREATE_DT` datetime NOT NULL DEFAULT current_timestamp() COMMENT '생성일',
  `UPDATE_DT` datetime DEFAULT NULL COMMENT '수정일',
  `DEL_YN` tinyint(1) DEFAULT NULL COMMENT '삭제여부',
  `DELETE_DT` datetime DEFAULT NULL COMMENT '삭제일',
  PRIMARY KEY (`PID`),
  UNIQUE KEY `UK` (`UID`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `login_tb`
--

LOCK TABLES `login_tb` WRITE;
/*!40000 ALTER TABLE `login_tb` DISABLE KEYS */;
INSERT INTO `login_tb` VALUES (1,'Master','1234',1,0,1,'2025-02-20 08:48:03',NULL,0,NULL),(2,'user','1234',0,0,1,'2025-02-20 09:32:54','2025-02-20 09:32:54',0,NULL),(3,'user01','1234',0,0,0,'2025-02-20 16:36:47','2025-02-20 16:36:47',0,NULL),(4,'user02','5555111',0,0,1,'2025-02-20 16:36:58','2025-02-20 16:57:15',1,'2025-02-20 16:57:15'),(6,'Manager','1234',0,1,1,'2025-02-21 10:51:17','2025-02-21 10:51:17',0,NULL);
/*!40000 ALTER TABLE `login_tb` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `pcroom_tb`
--

DROP TABLE IF EXISTS `pcroom_tb`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `pcroom_tb` (
  `PID` int(11) NOT NULL AUTO_INCREMENT,
  `IP` varchar(25) NOT NULL COMMENT '아이피 주소',
  `PORT` int(11) NOT NULL COMMENT '포트번호',
  `NAME` varchar(50) NOT NULL COMMENT '피시방 상호',
  `ADDR` varchar(100) NOT NULL COMMENT '주소',
  `SEATNUMBER` int(11) DEFAULT NULL COMMENT '좌석수',
  `PRICE` float NOT NULL DEFAULT 0 COMMENT '요금제 가격',
  `PRICE_PERCENT` varchar(10) DEFAULT NULL COMMENT 'PC 요금제 비율',
  `PC_SPEC` varchar(100) DEFAULT NULL COMMENT 'PC 사양',
  `TELECOM` varchar(30) DEFAULT NULL COMMENT '통신사',
  `MEMO` varchar(255) DEFAULT NULL COMMENT '메모',
  `CREATE_DT` datetime NOT NULL DEFAULT current_timestamp() COMMENT '생성일',
  `UPDATE_DT` datetime DEFAULT NULL COMMENT '수정일',
  `DEL_YN` tinyint(1) DEFAULT 0 COMMENT '삭제유무',
  `DELETE_DT` datetime DEFAULT NULL COMMENT '삭제일',
  `COUNTRYTB_ID` int(11) NOT NULL COMMENT '(도/시) 테이블 키',
  `CITYTB_ID` int(11) NOT NULL COMMENT '(시/군/구) 테이블 키',
  `TOWNTB_ID` int(11) NOT NULL COMMENT '(읍/면/동) 테이블 키',
  PRIMARY KEY (`PID`),
  UNIQUE KEY `UK` (`IP`,`NAME`,`PORT`,`ADDR`) USING BTREE,
  KEY `fk_pcroom_city` (`CITYTB_ID`),
  KEY `fk_pcroom_country` (`COUNTRYTB_ID`),
  KEY `fk_pcroom_town` (`TOWNTB_ID`),
  CONSTRAINT `fk_pcroom_city` FOREIGN KEY (`CITYTB_ID`) REFERENCES `city_tb` (`PID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_pcroom_country` FOREIGN KEY (`COUNTRYTB_ID`) REFERENCES `country_tb` (`PID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_pcroom_town` FOREIGN KEY (`TOWNTB_ID`) REFERENCES `town_tb` (`PID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `pcroom_tb`
--

LOCK TABLES `pcroom_tb` WRITE;
/*!40000 ALTER TABLE `pcroom_tb` DISABLE KEYS */;
INSERT INTO `pcroom_tb` VALUES (1,'123.2.156.148',5245,'테스트상호','경기도 하남시 미사대로',30,3000,'30%','몰라','SK','테스트용','2025-02-21 12:57:49',NULL,0,NULL,1,1,1),(2,'123.2.156.148',5245,'테스트상호1','경기도 하남시 미사대로',30,3000,'30%','몰라','SK','테스트용','2025-02-21 12:58:34',NULL,0,NULL,1,1,2),(3,'123.2.123.123',2222,'부산사업장','부산광역시 사상구 주례동',30,3000,'30%','몰라','SK','테스트2','2025-02-21 13:00:13',NULL,0,NULL,3,2,3),(4,'123.2.123.123',2222,'부산 화명사업장','부산광역시 북구 화명동',30,3000,'30%','몰라','SK','테스트3','2025-02-21 13:01:09',NULL,0,NULL,3,3,4),(5,'123.2.123.123',2222,'서울 개포 사업장','서울시 강남구 개포동',30,3000,'30%','몰라','SK','테스트3','2025-02-21 13:01:50',NULL,0,NULL,4,4,5);
/*!40000 ALTER TABLE `pcroom_tb` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `pinglog_tb`
--

DROP TABLE IF EXISTS `pinglog_tb`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `pinglog_tb` (
  `PID` int(11) NOT NULL AUTO_INCREMENT,
  `USED_PC` int(11) NOT NULL DEFAULT 0 COMMENT '사용대수',
  `PRICE` float NOT NULL DEFAULT 0 COMMENT '총금액',
  `CREATE_DT` datetime DEFAULT current_timestamp() COMMENT '생성일',
  `UPDATE_DT` datetime DEFAULT NULL COMMENT '수정일',
  `DEL_YN` tinyint(1) DEFAULT 0 COMMENT '삭제유무',
  `DELETE_DT` datetime DEFAULT NULL COMMENT '삭제일',
  `PCROOMTB_ID` int(11) NOT NULL COMMENT 'PC방 테이블 키',
  `COUNTRYTB_ID` int(11) NOT NULL COMMENT '(도/시) 테이블 키',
  `CITYTB_ID` int(11) NOT NULL COMMENT '(시/군/구) 테이블 키',
  `TOWNTB_ID` int(11) NOT NULL COMMENT '(읍/면/동) 테이블 키',
  `TIMETB_ID` int(11) NOT NULL COMMENT '시간 테이블 키',
  PRIMARY KEY (`PID`),
  KEY `fk_PINGLOG_pcroom202502191019` (`PCROOMTB_ID`),
  KEY `fk_PLINGLOG_city202502191020` (`CITYTB_ID`),
  KEY `fk_PLINGLOG_country202502191020` (`COUNTRYTB_ID`),
  KEY `fk_PLINGLOG_town202502191020` (`TOWNTB_ID`),
  KEY `fk_PLINGLOG_time202502191020` (`TIMETB_ID`),
  CONSTRAINT `fk_PINGLOG_pcroom202502191019` FOREIGN KEY (`PCROOMTB_ID`) REFERENCES `pcroom_tb` (`PID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_PLINGLOG_city202502191020` FOREIGN KEY (`CITYTB_ID`) REFERENCES `city_tb` (`PID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_PLINGLOG_country202502191020` FOREIGN KEY (`COUNTRYTB_ID`) REFERENCES `country_tb` (`PID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_PLINGLOG_time202502191020` FOREIGN KEY (`TIMETB_ID`) REFERENCES `time_tb` (`PID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_PLINGLOG_town202502191020` FOREIGN KEY (`TOWNTB_ID`) REFERENCES `town_tb` (`PID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='핑 정보';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `pinglog_tb`
--

LOCK TABLES `pinglog_tb` WRITE;
/*!40000 ALTER TABLE `pinglog_tb` DISABLE KEYS */;
/*!40000 ALTER TABLE `pinglog_tb` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `time_tb`
--

DROP TABLE IF EXISTS `time_tb`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `time_tb` (
  `PID` int(11) NOT NULL AUTO_INCREMENT,
  `TIME` time DEFAULT NULL COMMENT '00:00:00 ~ 24:00:00 / 30분단위',
  PRIMARY KEY (`PID`)
) ENGINE=InnoDB AUTO_INCREMENT=49 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `time_tb`
--

LOCK TABLES `time_tb` WRITE;
/*!40000 ALTER TABLE `time_tb` DISABLE KEYS */;
INSERT INTO `time_tb` VALUES (1,'00:00:00'),(2,'00:30:00'),(3,'01:00:00'),(4,'01:30:00'),(5,'02:00:00'),(6,'02:30:00'),(7,'03:00:00'),(8,'03:30:00'),(9,'04:00:00'),(10,'04:30:00'),(11,'05:00:00'),(12,'05:30:00'),(13,'06:00:00'),(14,'06:30:00'),(15,'07:00:00'),(16,'07:30:00'),(17,'08:00:00'),(18,'08:30:00'),(19,'09:00:00'),(20,'09:30:00'),(21,'10:00:00'),(22,'10:30:00'),(23,'11:00:00'),(24,'11:30:00'),(25,'12:00:00'),(26,'12:30:00'),(27,'13:00:00'),(28,'13:30:00'),(29,'14:00:00'),(30,'14:30:00'),(31,'15:00:00'),(32,'15:30:00'),(33,'16:00:00'),(34,'16:30:00'),(35,'17:00:00'),(36,'17:30:00'),(37,'18:00:00'),(38,'18:30:00'),(39,'19:00:00'),(40,'19:30:00'),(41,'20:00:00'),(42,'20:30:00'),(43,'21:00:00'),(44,'21:30:00'),(45,'22:00:00'),(46,'22:30:00'),(47,'23:00:00'),(48,'23:30:00');
/*!40000 ALTER TABLE `time_tb` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `town_tb`
--

DROP TABLE IF EXISTS `town_tb`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `town_tb` (
  `PID` int(11) NOT NULL AUTO_INCREMENT,
  `NAME` varchar(50) NOT NULL COMMENT '(읍/면/동) 명칭',
  `CREATE_DT` datetime NOT NULL DEFAULT current_timestamp() COMMENT '생성일',
  `UPDATE_DT` datetime DEFAULT NULL COMMENT '수정일',
  `DEL_YN` tinyint(1) DEFAULT 0 COMMENT '삭제유무',
  `DELETE_DT` datetime DEFAULT NULL COMMENT '삭제일',
  `COUNTYTB_ID` int(11) NOT NULL COMMENT '(도/시) 테이블 키',
  `CITYTB_ID` int(11) NOT NULL COMMENT '(시/군/구) 테이블 키',
  PRIMARY KEY (`PID`),
  UNIQUE KEY `UK` (`NAME`),
  KEY `fk_country202502192215` (`COUNTYTB_ID`),
  KEY `fk_city202502192215` (`CITYTB_ID`),
  CONSTRAINT `fk_city202502192215` FOREIGN KEY (`CITYTB_ID`) REFERENCES `city_tb` (`PID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_country202502192215` FOREIGN KEY (`COUNTYTB_ID`) REFERENCES `country_tb` (`PID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `town_tb`
--

LOCK TABLES `town_tb` WRITE;
/*!40000 ALTER TABLE `town_tb` DISABLE KEYS */;
INSERT INTO `town_tb` VALUES (1,'망월동','2025-02-21 12:57:49',NULL,0,NULL,1,1),(2,'상일동','2025-02-21 12:58:34',NULL,0,NULL,1,1),(3,'주례동동','2025-02-21 13:00:13',NULL,0,NULL,3,2),(4,'화명동동','2025-02-21 13:01:09',NULL,0,NULL,3,3),(5,'개포동','2025-02-21 13:01:50',NULL,0,NULL,4,4);
/*!40000 ALTER TABLE `town_tb` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-21 16:02:44

/*
Navicat MySQL Data Transfer

Source Server         : localhost
Source Server Version : 50723
Source Host           : localhost:3306
Source Database       : qipai

Target Server Type    : MYSQL
Target Server Version : 50723
File Encoding         : 65001

Date: 2019-10-14 21:03:24
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for account
-- ----------------------------
DROP TABLE IF EXISTS `account`;
CREATE TABLE `account` (
  `uid` int(10) unsigned NOT NULL AUTO_INCREMENT COMMENT '玩家唯一uid',
  `username` varchar(15) NOT NULL DEFAULT '' COMMENT '玩家姓名',
  `password` varchar(15) NOT NULL DEFAULT '' COMMENT '密码',
  `regtime` datetime NOT NULL COMMENT '注册时间',
  PRIMARY KEY (`uid`),
  KEY `username` (`username`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of account
-- ----------------------------

-- ----------------------------
-- Table structure for friend
-- ----------------------------
DROP TABLE IF EXISTS `friend`;
CREATE TABLE `friend` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `uid` int(10) unsigned DEFAULT NULL,
  `uidF` int(11) unsigned DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uids` (`uid`,`uidF`) USING BTREE,
  KEY `uid` (`uid`) USING BTREE,
  KEY `uidF` (`uidF`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of friend
-- ----------------------------

-- ----------------------------
-- Table structure for game
-- ----------------------------
DROP TABLE IF EXISTS `game`;
CREATE TABLE `game` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `roleId` int(11) unsigned DEFAULT NULL COMMENT '比赛创建者id',
  `roleName` varchar(15) CHARACTER SET utf8 DEFAULT NULL,
  `gameType` int(11) DEFAULT '0' COMMENT '游戏类型',
  `gameName` varchar(30) CHARACTER SET utf8 DEFAULT NULL COMMENT '比赛名',
  `gameNotice` varchar(50) CHARACTER SET utf8 DEFAULT NULL COMMENT '游戏介绍',
  `gameParam` text CHARACTER SET utf8 COMMENT '游戏其他参数',
  `createTime` datetime DEFAULT NULL COMMENT '创建时间',
  `startTime` datetime DEFAULT NULL COMMENT '开始时间',
  `endTime` datetime DEFAULT NULL COMMENT '结束比赛，结算时间',
  `closeTime` datetime DEFAULT NULL COMMENT '关闭显示时间',
  `rankNum` int(11) DEFAULT NULL COMMENT '排行榜多少人',
  `password` varchar(10) CHARACTER SET utf8 DEFAULT NULL COMMENT '密码',
  `award` text CHARACTER SET utf8,
  `state` int(11) DEFAULT NULL COMMENT '比赛状态：1即将开始，2进行中，3已结算，4已关闭',
  `gmClosed` int(11) DEFAULT '0' COMMENT 'gm后台关闭，1是，0否',
  `getDouzi` int(11) unsigned DEFAULT '0' COMMENT '该比赛所获得的门票和桌费',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of game
-- ----------------------------

-- ----------------------------
-- Table structure for gametypestate
-- ----------------------------
DROP TABLE IF EXISTS `gametypestate`;
CREATE TABLE `gametypestate` (
  `gameType` int(11) NOT NULL COMMENT '游戏类型',
  `state` int(11) DEFAULT NULL COMMENT '游戏类型状态：1正常，2待开放，3关闭',
  `gameTypeName` text COMMENT '类型名（方便看，不是客户端看到的）',
  PRIMARY KEY (`gameType`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of gametypestate
-- ----------------------------
INSERT INTO `gametypestate` VALUES ('11', '2', '三人斗地主');
INSERT INTO `gametypestate` VALUES ('21', '2', '二人麻将');
INSERT INTO `gametypestate` VALUES ('22', '2', '四人麻将');
INSERT INTO `gametypestate` VALUES ('31', '1', '中国象棋');
INSERT INTO `gametypestate` VALUES ('41', '1', '五子棋');

-- ----------------------------
-- Table structure for mail
-- ----------------------------
DROP TABLE IF EXISTS `mail`;
CREATE TABLE `mail` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `uid` int(10) unsigned DEFAULT NULL,
  `topic` varchar(20) DEFAULT '',
  `content` varchar(50) DEFAULT '',
  `items` varchar(255) DEFAULT NULL,
  `status` tinyint(4) DEFAULT NULL COMMENT '0未读，1已读，2已领奖',
  `createTime` datetime DEFAULT NULL,
  `expireTime` datetime DEFAULT NULL,
  `sendUid` int(10) unsigned DEFAULT NULL,
  `sendName` varchar(15) DEFAULT '',
  PRIMARY KEY (`id`),
  KEY `uid` (`uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of mail
-- ----------------------------

-- ----------------------------
-- Table structure for mail_all
-- ----------------------------
DROP TABLE IF EXISTS `mail_all`;
CREATE TABLE `mail_all` (
  `uid` int(10) unsigned NOT NULL,
  `readIds` text,
  `getAwardIds` text,
  `delIds` text,
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of mail_all
-- ----------------------------

-- ----------------------------
-- Table structure for player
-- ----------------------------
DROP TABLE IF EXISTS `player`;
CREATE TABLE `player` (
  `uid` int(10) unsigned NOT NULL DEFAULT '1' COMMENT '玩家uid',
  `nickname` varchar(15) NOT NULL DEFAULT 'fdsf' COMMENT '昵称',
  `sex` tinyint(4) DEFAULT '1' COMMENT '性别',
  `headId` tinyint(4) DEFAULT NULL COMMENT '头像id',
  `signature` varchar(100) DEFAULT NULL COMMENT '个性签名',
  `loginTime` datetime DEFAULT NULL COMMENT '登录时间',
  `regTime` datetime DEFAULT NULL COMMENT '注册时间',
  `loginDays` tinyint(4) DEFAULT NULL COMMENT '连续登录天数',
  `ifGetAward` tinyint(4) DEFAULT NULL COMMENT '今天是否已签到领奖',
  `bag` text COMMENT '背包',
  `gameData` text COMMENT '游戏统计数据',
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of player
-- ----------------------------

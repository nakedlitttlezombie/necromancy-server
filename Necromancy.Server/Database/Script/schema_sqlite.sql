CREATE TABLE IF NOT EXISTS `setting` (
  `key`   TEXT NOT NULL,
  `value` TEXT NOT NULL,
  PRIMARY KEY (`key`)
);

CREATE TABLE IF NOT EXISTS `account` (
  `id`               INTEGER PRIMARY KEY AUTOINCREMENT  NOT NULL,
  `name`             TEXT                               NOT NULL,
  `normal_name`      TEXT                               NOT NULL,
  `hash`             TEXT                               NOT NULL,
  `mail`             TEXT                               NOT NULL,
  `mail_verified`    INTEGER                            NOT NULL,
  `mail_verified_at` DATETIME   DEFAULT                 NULL,
  `mail_token`       TEXT       DEFAULT                 NULL,
  `password_token`   TEXT       DEFAULT                 NULL,
  `state`            INTEGER                            NOT NULL,
  `last_login`       DATETIME   DEFAULT                 NULL,
  `created`          DATETIME                           NOT NULL,
  CONSTRAINT `uq_account_name` UNIQUE (`name`),
  CONSTRAINT `uq_account_normal_name` UNIQUE (`normal_name`),
  CONSTRAINT `uq_account_mail` UNIQUE (`mail`)
);

CREATE TABLE IF NOT EXISTS `nec_soul` (
  `id`         INTEGER PRIMARY KEY AUTOINCREMENT    NOT NULL,
  `account_id` INTEGER                              NOT NULL,
  `name`       TEXT                                 NOT NULL,
  `level`      INTEGER                              NOT NULL,
  `created`    DATETIME                             NOT NULL,  
  `password`   TEXT         DEFAULT                 NULL,
  CONSTRAINT `fk_nec_soul_account_id` FOREIGN KEY (`account_id`) REFERENCES `account` (`id`),
  CONSTRAINT `uq_nec_soul_name` UNIQUE (`name`)
);

CREATE TABLE IF NOT EXISTS `nec_skilltree_item` (
  `id`         INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
  `skill_id`   INTEGER                           NOT NULL,
  `char_id`    INTEGER                           NOT NULL,
  `level`      INTEGER                           NOT NULL
);

CREATE TABLE IF NOT EXISTS `nec_shortcut_bar` (
  `id`      INTEGER PRIMARY KEY AUTOINCREMENT       NOT NULL,
  `slot0`   INTEGER                                 NOT NULL,
  `slot1`   INTEGER                                 NOT NULL,
  `slot2`   INTEGER                                 NOT NULL,
  `slot3`   INTEGER                                 NOT NULL,
  `slot4`   INTEGER                                 NOT NULL,
  `slot5`   INTEGER                                 NOT NULL,
  `slot6`   INTEGER                                 NOT NULL,
  `slot7`   INTEGER                                 NOT NULL,
  `slot8`   INTEGER                                 NOT NULL,
  `slot9`   INTEGER                                 NOT NULL,
  `action0` INTEGER                                 NOT NULL,
  `action1` INTEGER                                 NOT NULL,
  `action2` INTEGER                                 NOT NULL,
  `action3` INTEGER                                 NOT NULL,
  `action4` INTEGER                                 NOT NULL,
  `action5` INTEGER                                 NOT NULL,
  `action6` INTEGER                                 NOT NULL,
  `action7` INTEGER                                 NOT NULL,
  `action8` INTEGER                                 NOT NULL,
  `action9` INTEGER                                 NOT NULL
);

CREATE TABLE IF NOT EXISTS `nec_character` (
  `id`                INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, 
  `account_id`        INTEGER                           NOT NULL,
  `soul_id`           INTEGER                           NOT NULL,
  `slot`              INTEGER                           NOT NULL,
  `map_id`            INTEGER                           NOT NULL,
  `x`                 REAL                              NOT NULL,
  `y`                 REAL                              NOT NULL,
  `z`                 REAL                              NOT NULL,
  `name`              TEXT                              NOT NULL,
  `race_id`           INTEGER                           NOT NULL,
  `sex_id`            INTEGER                           NOT NULL,
  `hair_id`           INTEGER                           NOT NULL,
  `hair_color_id`     INTEGER                           NOT NULL,
  `face_id`           INTEGER                           NOT NULL,
  `alignment_id`      INTEGER                           NOT NULL,
  `strength`          INTEGER                           NOT NULL,
  `vitality`          INTEGER                           NOT NULL,
  `dexterity`         INTEGER                           NOT NULL,
  `agility`           INTEGER                           NOT NULL,
  `intelligence`      INTEGER                           NOT NULL,
  `piety`             INTEGER                           NOT NULL,
  `luck`              INTEGER                           NOT NULL,
  `class_id`          INTEGER                           NOT NULL,
  `level`             INTEGER                           NOT NULL,
  `shortcut_bar0_id`  INTEGER                           NOT NULL,
  `shortcut_bar1_id`  INTEGER                           NOT NULL,
  `shortcut_bar2_id`  INTEGER                           NOT NULL,
  `shortcut_bar3_id`  INTEGER                           NOT NULL,
  `shortcut_bar4_id`  INTEGER                           NOT NULL,
  `created`           DATETIME                          NOT NULL,
  CONSTRAINT `fk_nec_character_account_id` FOREIGN KEY (`account_id`) REFERENCES `account` (`id`),
  CONSTRAINT `fk_nec_character_soul_id` FOREIGN KEY (`soul_id`) REFERENCES `nec_soul` (`id`),
  CONSTRAINT `uq_nec_character_soul_id_name` UNIQUE (`soul_id`, `name`),
  CONSTRAINT `uq_nec_character_soul_id_slot` UNIQUE (`soul_id`, `slot`)
);

CREATE TABLE IF NOT EXISTS `nec_npc_spawn` (
  `id`         INTEGER PRIMARY KEY NOT NULL, 
  `npc_id`     INTEGER             NOT NULL,
  `model_id`   INTEGER             NOT NULL,
  `level`      INTEGER             NOT NULL,
  `name`       TEXT                NOT NULL,
  `title`      TEXT                NOT NULL,
  `map_id`     INTEGER             NOT NULL,
  `x`          REAL                NOT NULL,
  `y`          REAL                NOT NULL,
  `z`          REAL                NOT NULL,
  `active`     INTEGER             NOT NULL,
  `heading`    INTEGER             NOT NULL,
  `size`       INTEGER             NOT NULL,
  `visibility` INTEGER             NOT NULL,
  `created`    DATETIME            NOT NULL,
  `updated`    DATETIME            NOT NULL,
  `icon`       INTEGER             NOT NULL,
  `status`     INTEGER             NOT NULL,
  `status_x`   INTEGER             NOT NULL,
  `status_y`   INTEGER             NOT NULL,
  `status_z`   INTEGER             NOT NULL
);

CREATE TABLE IF NOT EXISTS `nec_monster_coords` (
  `id`         INTEGER PRIMARY KEY NOT NULL, 
  `monster_id` INTEGER             NOT NULL,
  `map_id`     INTEGER             NOT NULL,
  `coord_idx`  INTEGER             NOT NULL,
  `x`          REAL                NOT NULL,
  `y`          REAL                NOT NULL,
  `z`          REAL                NOT NULL
);

CREATE TABLE IF NOT EXISTS `nec_monster_spawn` (
  `id`         INTEGER PRIMARY KEY NOT NULL, 
  `monster_id` INTEGER             NOT NULL,
  `model_id`   INTEGER             NOT NULL,
  `level`      INTEGER             NOT NULL,
  `name`       TEXT                NOT NULL,
  `title`      TEXT                NOT NULL,
  `map_id`     INTEGER             NOT NULL,
  `x`          REAL                NOT NULL,
  `y`          REAL                NOT NULL,
  `z`          REAL                NOT NULL,
  `active`     INTEGER             NOT NULL,
  `heading`    INTEGER             NOT NULL,
  `size`       INTEGER             NOT NULL,
  `created`    DATETIME            NOT NULL,
  `updated`    DATETIME            NOT NULL
);
 
CREATE TABLE IF NOT EXISTS "nec_item" (
	"row_id"	        INTEGER     NOT NULL PRIMARY KEY AUTOINCREMENT,
	"name"	            TEXT        NOT NULL,
	"type"	            INTEGER     NOT NULL,
	"bitmask"	        INTEGER     NOT NULL,
	"count"	            INTEGER     NOT NULL,
	"state"	            INTEGER     NOT NULL,
	"icon"	            INTEGER     NOT NULL,
	"hair_override"	    INTEGER     NOT NULL,
	"face_override"	    INTEGER     NOT NULL,
	"durability"	    INTEGER     NOT NULL,
	"max_durability"	INTEGER     NOT NULL,
	"weight"	        INTEGER     NOT NULL,
	"physics"	        INTEGER     NOT NULL,
	"magic"	            INTEGER     NOT NULL,
	"enchant_id"	    INTEGER     NOT NULL,
	"ac"	            INTEGER     NOT NULL,
	"date_end_protect"	INTEGER     NOT NULL,
	"hardness"	        INTEGER     NOT NULL,
    "level"             INTEGER     NOT NULL
);

CREATE TABLE IF NOT EXISTS `nec_block_list` (
	`id`	        INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	`soul_id`	    INTEGER NOT NULL,
	`block_soul_id`	INTEGER NOT NULL,
	FOREIGN KEY(`soul_id`) REFERENCES `nec_soul`(`id`),
	FOREIGN KEY(`block_soul_id`) REFERENCES `nec_soul`(`id`)
);

CREATE TABLE IF NOT EXISTS `nec_union` (
	`id`	                INTEGER     NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	`name`	                TEXT        NOT NULL,
	`union_leader_id`	    INTEGER     NOT NULL,
	`union_sub_leader1_id`	INTEGER,
	`union_sub_leader2_id`	INTEGER,
	`level`	                INTEGER     NOT NULL,
	`current_exp`	        INTEGER     NOT NULL,
	`next_level_exp`	    INTEGER     NOT NULL,
	`member_limit_increase`	INTEGER     NOT NULL,
	`cape_design_id`	    INTEGER,
	`union_news`	        TEXT,
	`created`	            DATETIME    NOT NULL,
	FOREIGN KEY(`union_leader_id`) REFERENCES `nec_character`(`id`)
);

CREATE TABLE IF NOT EXISTS `nec_union_member` (
	`id`	                    INTEGER     NOT NULL PRIMARY KEY AUTOINCREMENT,
	`union_id`	                INTEGER     NOT NULL,
	`character_id`	            INTEGER     NOT NULL,
	`member_priviledge_bitmask`	INTEGER     NOT NULL,
	`rank`	                    INTEGER     NOT NULL,
	`joined`	                DATETIME    NOT NULL
);

CREATE TABLE IF NOT EXISTS `nec_union_news` (
	`Id`	                INTEGER     NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	`character_soul_name`	STRING      NOT NULL,
	`character_name`	    STRING      NOT NULL,
	`activity`	            INTEGER     NOT NULL,
	`string3`	            STRING,
	`string4`	            STRING,
	`itemcount`	            INTEGER     NOT NULL
);

CREATE TABLE IF NOT EXISTS `nec_black_list` (
	`id`	                INTEGER     NOT NULL PRIMARY KEY AUTOINCREMENT,
	`soul_id`	            INTEGER     NOT NULL,
	`black_soul_id`	        INTEGER     NOT NULL,
	FOREIGN KEY(`black_soul_id`) REFERENCES `nec_soul`(`id`),
	FOREIGN KEY(`soul_id`) REFERENCES `nec_soul`(`id`)
);

CREATE TABLE IF NOT EXISTS `nec_friend_list` (
	`id`	            INTEGER     NOT NULL PRIMARY KEY AUTOINCREMENT,
	`soul_id`	        INTEGER     NOT NULL,
	`friend_soul_id`	INTEGER     NOT NULL,
	FOREIGN KEY(`soul_id`) REFERENCES `nec_soul`(`id`),
	FOREIGN KEY(`friend_soul_id`) REFERENCES `nec_soul`(`id`)
);



CREATE TABLE IF NOT EXISTS `inventory`(
   `bag_id`     INTEGER,
   `gold`       INTEGER,
   PRIMARY KEY(`bag_id`)
);

CREATE TABLE IF NOT EXISTS `slots_numbers`(
   `bag_id`      INTEGER,
   `bag_index`   INTEGER,
   PRIMARY KEY(`bag_id`, `bag_index`),
   FOREIGN KEY(`bag_id`) REFERENCES `Inventory`(`bag_id`)
);

CREATE TABLE IF NOT EXISTS `items`(
   `items_id`       BIGINT,
   `item_name`      TEXT        NOT NULL,
   `item_type`      INT,
   `physics`        SMALLINT,
   `magic`          SMALLINT,
   `enchant_id`     INT,
   `durab`          INT,
   `hardness`       TINYINT,
   `max_dur`        INT,
   `numbers`        TINYINT,
   `level`          TINYINT,
   `sp_level`       TINYINT,
   `weight`         INT,
   `state`          INT,
   PRIMARY KEY(`items_id`)
);


CREATE TABLE IF NOT EXISTS `items_in_slot`(
   `bag_id`      INT,
   `bag_index`   INT,
   `items_id`    INT,
   PRIMARY KEY(`bag_id`, `bag_index`, `items_id`),
   FOREIGN KEY(`bag_id`, `bag_index`) REFERENCES `SlotsNumbers`(`bag_id`, `bag_index`),
   FOREIGN KEY(`items_id`) REFERENCES Items(`items_id`)
);

 CREATE TABLE IF NOT EXISTS quest_request(
   quest_id                 INT,
   soul_level_mission       BYTE,
   quest_name               CHAR(50),
   quest_level              INT,
   time_limit               INT,
   quest_giver_name         CHAR(50),
   reward_exp               INT,
   reward_gold              INT,
   numbers_of_items         SMALLINT,
   items_type               INT,
   PRIMARY KEY(quest_id)
);

-- Table: nec_gimmick
CREATE TABLE IF NOT EXISTS `nec_gimmick_spawn` (
    `id`       INTEGER      NOT NULL PRIMARY KEY AUTOINCREMENT,
    `map_id`   INTEGER      NOT NULL,
    `x`        INTEGER      NOT NULL,
    `y`        INTEGER      NOT NULL,
    `z`        INTEGER      NOT NULL,
    `heading`  INTEGER      NOT NULL,
    `model_id` INTEGER      NOT NULL,
    `state`    INTEGER      NOT NULL,
    `created`  DATETIME,
    `updated`  DATETIME
);

-- Table: nec_map_transition
CREATE TABLE IF NOT EXISTS `nec_map_transition` (
    `id`                    INTEGER     NOT NULL PRIMARY KEY AUTOINCREMENT,
    `map_id`                INTEGER     NOT NULL,
    `transition_map_id`     INTEGER     NOT NULL,
    `x`                     REAL        NOT NULL,
    `y`                     REAL        NOT NULL,
    `z`                     REAL        NOT NULL,
    `maplink_heading`       INTEGER     NOT NULL,
    `maplink_color`         INTEGER     NOT NULL,
    `maplink_offset`        INTEGER     NOT NULL,
    `maplink_width`         INTEGER     NOT NULL,
    `distance`              INTEGER     NOT NULL,
    `left_x`                REAL        NOT NULL,
    `left_y`                REAL        NOT NULL,
    `left_z`                REAL        NOT NULL,
    `right_x`               REAL        NOT NULL,
    `right_y`               REAL        NOT NULL,
    `right_z`               REAL        NOT NULL,
    `invertedPos`           INTEGER     NOT NULL,
    `to_x`                  REAL        NOT NULL,
    `to_y`                  REAL        NOT NULL,
    `to_z`                  REAL        NOT NULL,
    `to_heading`            INTEGER     NOT NULL,
    `state`                 INTEGER     NOT NULL,
    `created`               DATETIME,
    `updated`               DATETIME
);
CREATE TABLE IF NOT EXISTS `nec_ggate_spawn` (
  `id`          INTEGER PRIMARY KEY NOT NULL, 
  `serial_id`   INTEGER             NOT NULL,
  `interaction` INTEGER             NOT NULL,
  `name`        TEXT                NOT NULL,
  `title`       TEXT                NOT NULL,
  `map_id`      INTEGER             NOT NULL,
  `x`           REAL                NOT NULL,
  `y`           REAL                NOT NULL,
  `z`           REAL                NOT NULL,
  `heading`     INTEGER             NOT NULL,
  `model_id`    INTEGER             NOT NULL,
  `size`        INTEGER             NOT NULL,
  `active`      INTEGER             NOT NULL,
  `glow`        INTEGER             NOT NULL,
  `created`     DATETIME            NOT NULL,
  `updated`     DATETIME            NOT NULL
);

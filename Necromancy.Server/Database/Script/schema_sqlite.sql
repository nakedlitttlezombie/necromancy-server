CREATE TABLE IF NOT EXISTS `setting`
(
    `key`   TEXT NOT NULL,
    `value` TEXT NOT NULL,
    PRIMARY KEY (`key`)
);

CREATE TABLE IF NOT EXISTS `account`
(
    `id`               INTEGER  NOT NULL PRIMARY KEY AUTOINCREMENT,
    `name`             TEXT     NOT NULL,
    `normal_name`      TEXT     NOT NULL,
    `hash`             TEXT     NOT NULL,
    `mail`             TEXT     NOT NULL,
    `mail_verified`    INTEGER  NOT NULL,
    `mail_verified_at` DATETIME DEFAULT NULL,
    `mail_token`       TEXT     DEFAULT NULL,
    `password_token`   TEXT     DEFAULT NULL,
    `state`            INTEGER  NOT NULL,
    `last_login`       DATETIME DEFAULT NULL,
    `created`          DATETIME NOT NULL,
    CONSTRAINT `uq_account_name` UNIQUE (`name`),
    CONSTRAINT `uq_account_normal_name` UNIQUE (`normal_name`),
    CONSTRAINT `uq_account_mail` UNIQUE (`mail`)
);

CREATE TABLE IF NOT EXISTS `nec_soul` (
	`id`	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	`account_id`	INTEGER NOT NULL,
	`name`	TEXT NOT NULL,
	`level`	INTEGER NOT NULL,
	`created`	DATETIME NOT NULL,
	`password`	TEXT DEFAULT NULL,
	`experience_current`	INTEGER NOT NULL DEFAULT (0),
	`warehouse_gold`	INTEGER NOT NULL DEFAULT (0),
	`criminal_level`	INTEGER NOT NULL DEFAULT (0),
	`points_current`	INTEGER NOT NULL DEFAULT (0),
	`material_life`	INTEGER NOT NULL DEFAULT (0),
	`material_reincarnation`	INTEGER NOT NULL DEFAULT (0),
	`material_lawful`	INTEGER NOT NULL DEFAULT (0),
	`material_chaos`	INTEGER NOT NULL DEFAULT (0),
	CONSTRAINT `uq_nec_soul_name` UNIQUE(`name`),
	CONSTRAINT `fk_nec_soul_account_id` FOREIGN KEY(`account_id`) REFERENCES `account`(`id`)
);

CREATE TABLE IF NOT EXISTS `nec_map`
(
    `id`          INTEGER NOT NULL PRIMARY KEY,
    `country`     TEXT    NOT NULL,
    `area`        TEXT    NOT NULL,
    `place`       TEXT    NOT NULL,
    `x`           INTEGER NOT NULL,
    `y`           INTEGER DEFAULT NULL,
    `z`           INTEGER DEFAULT NULL,
    `orientation` INTEGER DEFAULT NULL
);

CREATE TABLE IF NOT EXISTS `nec_character` (
	`id`	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	`account_id`	INTEGER NOT NULL,
	`soul_id`	INTEGER NOT NULL,
	`slot`	INTEGER NOT NULL,
	`map_id`	INTEGER NOT NULL,
	`x`	REAL NOT NULL,
	`y`	REAL NOT NULL,
	`z`	REAL NOT NULL,
	`name`	TEXT NOT NULL,
	`race_id`	INTEGER NOT NULL,
	`sex_id`	INTEGER NOT NULL,
	`hair_id`	INTEGER NOT NULL,
	`hair_color_id`	INTEGER NOT NULL,
	`face_id`	INTEGER NOT NULL,
	`strength`	INTEGER NOT NULL,
	`vitality`	INTEGER NOT NULL,
	`dexterity`	INTEGER NOT NULL,
	`agility`	INTEGER NOT NULL,
	`intelligence`	INTEGER NOT NULL,
	`piety`	INTEGER NOT NULL,
	`luck`	INTEGER NOT NULL,
    `points_lawful` INTEGER NOT NULL DEFAULT (0),
    `points_neutral` INTEGER NOT NULL DEFAULT (0),
    `points_chaos` INTEGER NOT NULL DEFAULT (0),
	`class_id`	INTEGER NOT NULL,
	`level`	INTEGER NOT NULL,
	`created`	DATETIME NOT NULL,
	`hp_current`	INTEGER NOT NULL DEFAULT (0),
	`mp_current`	INTEGER NOT NULL DEFAULT (0),
	`gold`	INTEGER NOT NULL DEFAULT (0),
	`condition_current`	INTEGER NOT NULL DEFAULT (0),
	`channel`	INTEGER NOT NULL DEFAULT (0),
	`face_arrange_id`	INTEGER NOT NULL DEFAULT (0),
	`voice_id`	INTEGER NOT NULL DEFAULT (0),
	`experience_current`	INTEGER NOT NULL DEFAULT (0),
	`skill_points`	INTEGER NOT NULL DEFAULT (0),
	CONSTRAINT `uq_nec_character_soul_id_name` UNIQUE(`soul_id`,`name`),
	CONSTRAINT `fk_nec_character_account_id` FOREIGN KEY(`account_id`) REFERENCES `account`(`id`),
	CONSTRAINT `fk_nec_character_soul_id` FOREIGN KEY(`soul_id`) REFERENCES `nec_soul`(`id`),
	CONSTRAINT `uq_nec_character_soul_id_slot` UNIQUE(`soul_id`,`slot`),
	CONSTRAINT `fk_nec_character_map_id` FOREIGN KEY(`map_id`) REFERENCES `nec_map`(`id`)
);

CREATE TABLE IF NOT EXISTS `nec_npc_spawn`
(
    `id`         INTEGER  NOT NULL PRIMARY KEY AUTOINCREMENT,
    `npc_id`     INTEGER  NOT NULL,
    `model_id`   INTEGER  NOT NULL,
    `level`      INTEGER  NOT NULL,
    `name`       TEXT     NOT NULL,
    `title`      TEXT     NOT NULL,
    `map_id`     INTEGER  NOT NULL,
    `x`          REAL     NOT NULL,
    `y`          REAL     NOT NULL,
    `z`          REAL     NOT NULL,
    `active`     INTEGER  NOT NULL,
    `heading`    INTEGER  NOT NULL,
    `size`       INTEGER  NOT NULL,
    `visibility` INTEGER  NOT NULL,
    `created`    DATETIME NOT NULL,
    `updated`    DATETIME NOT NULL,
    `icon`       INTEGER  NOT NULL,
    `status`     INTEGER  NOT NULL,
    `status_x`   INTEGER  NOT NULL,
    `status_y`   INTEGER  NOT NULL,
    `status_z`   INTEGER  NOT NULL
);

CREATE TABLE IF NOT EXISTS `nec_monster_spawn`
(
    `id`         INTEGER  NOT NULL PRIMARY KEY AUTOINCREMENT,
    `monster_id` INTEGER  NOT NULL,
    `model_id`   INTEGER  NOT NULL,
    `level`      INTEGER  NOT NULL,
    `name`       TEXT     NOT NULL,
    `title`      TEXT     NOT NULL,
    `map_id`     INTEGER  NOT NULL,
    `x`          REAL     NOT NULL,
    `y`          REAL     NOT NULL,
    `z`          REAL     NOT NULL,
    `active`     INTEGER  NOT NULL,
    `heading`    INTEGER  NOT NULL,
    `size`       INTEGER  NOT NULL,
    `created`    DATETIME NOT NULL,
    `updated`    DATETIME NOT NULL
);

CREATE TABLE IF NOT EXISTS `nec_skilltree_item`
(
    `id`       INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    `skill_id` INTEGER                           NOT NULL,
    `char_id`  INTEGER                           NOT NULL,
    `level`    INTEGER                           NOT NULL
);

CREATE TABLE IF NOT EXISTS `nec_shortcut_bar` (
	`character_id`	INTEGER NOT NULL,
	`bar_num`	INTEGER NOT NULL,
	`slot_num`	INTEGER NOT NULL,
	`shortcut_type`	INTEGER NOT NULL,
	`shortcut_id`	INTEGER NOT NULL,
	PRIMARY KEY(`character_id`,`bar_num`,`slot_num`),
	FOREIGN KEY(`character_id`) REFERENCES `nec_character`(`id`) ON DELETE CASCADE);

CREATE TABLE IF NOT EXISTS `nec_monster_coords`
(
    `id`         INTEGER PRIMARY KEY NOT NULL,
    `monster_id` INTEGER             NOT NULL,
    `map_id`     INTEGER             NOT NULL,
    `coord_idx`  INTEGER             NOT NULL,
    `x`          REAL                NOT NULL,
    `y`          REAL                NOT NULL,
    `z`          REAL                NOT NULL
);


CREATE TABLE IF NOT EXISTS `nec_block_list`
(
    `id`            INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    `soul_id`       INTEGER NOT NULL,
    `block_soul_id` INTEGER NOT NULL,
    FOREIGN KEY (`soul_id`) REFERENCES `nec_soul` (`id`),
    FOREIGN KEY (`block_soul_id`) REFERENCES `nec_soul` (`id`)
);

CREATE TABLE IF NOT EXISTS `nec_union`
(
    `id`                        INTEGER  NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
    `name`                      TEXT     NOT NULL,
    `leader_character_id`       INTEGER  NOT NULL,
    `subleader1_character_id`   INTEGER,
    `subleader2_character_id`   INTEGER,
    `level`                     INTEGER  NOT NULL,
    `current_exp`               INTEGER  NOT NULL,
    `next_level_exp`            INTEGER  NOT NULL,
    `member_limit_increase`     INTEGER  NOT NULL,
    `cape_design_id`            INTEGER,
    `union_news`                TEXT,
    `created`                   DATETIME NOT NULL,
    CONSTRAINT `fk_nec_union_leader_character_id` FOREIGN KEY (`leader_character_id`) REFERENCES `nec_character` (`id`)
    --CONSTRAINT `fk_nec_union_subleader1_character_id` FOREIGN KEY (`subleader1_character_id`) REFERENCES `nec_character` (`id`)
    --CONSTRAINT `fk_nec_union_subleader2_character_id` FOREIGN KEY (`subleader2_character_id`) REFERENCES `nec_character` (`id`)
);

CREATE TABLE IF NOT EXISTS `nec_union_member`
(
    `id`                        INTEGER  NOT NULL PRIMARY KEY AUTOINCREMENT,
    `union_id`                  INTEGER  NOT NULL,
    `character_id`              INTEGER  NOT NULL,
    `member_priviledge_bitmask` INTEGER  NOT NULL,
    `rank`                      INTEGER  NOT NULL,
    `joined`                    DATETIME NOT NULL
);

CREATE TABLE IF NOT EXISTS `nec_union_news`
(
    `Id`                  INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
    `character_soul_name` STRING  NOT NULL,
    `character_name`      STRING  NOT NULL,
    `activity`            INTEGER NOT NULL,
    `string3`             STRING,
    `string4`             STRING,
    `itemcount`           INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS `nec_black_list`
(
    `id`            INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    `soul_id`       INTEGER NOT NULL,
    `black_soul_id` INTEGER NOT NULL,
    FOREIGN KEY (`black_soul_id`) REFERENCES `nec_soul` (`id`),
    FOREIGN KEY (`soul_id`) REFERENCES `nec_soul` (`id`)
);

CREATE TABLE IF NOT EXISTS `nec_friend_list`
(
    `id`             INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    `soul_id`        INTEGER NOT NULL,
    `friend_soul_id` INTEGER NOT NULL,
    FOREIGN KEY (`soul_id`) REFERENCES `nec_soul` (`id`),
    FOREIGN KEY (`friend_soul_id`) REFERENCES `nec_soul` (`id`)
);

CREATE TABLE IF NOT EXISTS `nec_gimmick_spawn`
(
    `id`       INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    `map_id`   INTEGER NOT NULL,
    `x`        INTEGER NOT NULL,
    `y`        INTEGER NOT NULL,
    `z`        INTEGER NOT NULL,
    `heading`  INTEGER NOT NULL,
    `model_id` INTEGER NOT NULL,
    `state`    INTEGER NOT NULL,
    `created`  DATETIME,
    `updated`  DATETIME
);

CREATE TABLE IF NOT EXISTS `nec_map_transition`
(
    `id`                INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    `map_id`            INTEGER NOT NULL,
    `transition_map_id` INTEGER NOT NULL,
    `x`                 REAL    NOT NULL,
    `y`                 REAL    NOT NULL,
    `z`                 REAL    NOT NULL,
    `maplink_heading`   INTEGER NOT NULL,
    `maplink_color`     INTEGER NOT NULL,
    `maplink_offset`    INTEGER NOT NULL,
    `maplink_width`     INTEGER NOT NULL,
    `distance`          INTEGER NOT NULL,
    `left_x`            REAL    NOT NULL,
    `left_y`            REAL    NOT NULL,
    `left_z`            REAL    NOT NULL,
    `right_x`           REAL    NOT NULL,
    `right_y`           REAL    NOT NULL,
    `right_z`           REAL    NOT NULL,
    `invertedPos`       INTEGER NOT NULL,
    `to_x`              REAL    NOT NULL,
    `to_y`              REAL    NOT NULL,
    `to_z`              REAL    NOT NULL,
    `to_heading`        INTEGER NOT NULL,
    `state`             INTEGER NOT NULL,
    `created`           DATETIME,
    `updated`           DATETIME
);

CREATE TABLE IF NOT EXISTS `nec_ggate_spawn`
(
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

--Inventory and Item Related tables

CREATE TABLE IF NOT EXISTS `nec_item_library` 
(
	`id`                    INTEGER PRIMARY KEY NOT NULL UNIQUE,
	`item_type`	            TEXT NOT NULL,
	`quality`	            TEXT NOT NULL,
	`_item_name_necromancy`	TEXT,
	`max_stack_size`	    INTEGER,
	`c3`	                TEXT,
	`es_hand_r`	            INTEGER NOT NULL DEFAULT 0,
	`es_hand_l`	            INTEGER NOT NULL DEFAULT 0,
	`es_quiver`	            INTEGER NOT NULL DEFAULT 0,
	`es_head`	            INTEGER NOT NULL DEFAULT 0,
	`es_body`	            INTEGER NOT NULL DEFAULT 0,
	`es_legs`	            INTEGER NOT NULL DEFAULT 0,
	`es_arms`	            INTEGER NOT NULL DEFAULT 0,
	`es_feet`	            INTEGER NOT NULL DEFAULT 0,
	`es_mantle`	            INTEGER NOT NULL DEFAULT 0,
	`es_ring`	            INTEGER NOT NULL DEFAULT 0,
	`es_earring`	        INTEGER NOT NULL DEFAULT 0,
	`es_necklace`	        INTEGER NOT NULL DEFAULT 0,
	`es_belt`	            INTEGER NOT NULL DEFAULT 0,
	`es_talkring`	        INTEGER NOT NULL DEFAULT 0,
	`es_avatar_head`	    INTEGER NOT NULL DEFAULT 0,
	`es_avatar_body`	    INTEGER NOT NULL DEFAULT 0,
	`es_avatar_legs`	    INTEGER NOT NULL DEFAULT 0,
	`es_avatar_arms`	    INTEGER NOT NULL DEFAULT 0,
	`es_avatar_feet`	    INTEGER NOT NULL DEFAULT 0,
    `es_avatar_mantle`      INTEGER NOT NULL DEFAULT 0,
	`req_hum_m`	            INTEGER NOT NULL DEFAULT 0,
	`req_hum_f`	            INTEGER NOT NULL DEFAULT 0,
	`req_elf_m`	            INTEGER NOT NULL DEFAULT 0,
	`req_elf_f`	            INTEGER NOT NULL DEFAULT 0,
	`req_dwf_m`	            INTEGER NOT NULL DEFAULT 0,
	`req_dwf_f`	            INTEGER NOT NULL DEFAULT 0,
	`req_por_m`	            INTEGER NOT NULL DEFAULT 0,
	`req_por_f`	            INTEGER NOT NULL DEFAULT 0,
	`req_gnm_m`	            INTEGER NOT NULL DEFAULT 0,
	`req_gnm_f`	            INTEGER NOT NULL DEFAULT 0,
	`req_fighter`	        INTEGER NOT NULL DEFAULT 0,
	`req_thief`	            INTEGER NOT NULL DEFAULT 0,
	`req_mage`	            INTEGER NOT NULL DEFAULT 0,
	`req_priest`	        INTEGER NOT NULL DEFAULT 0,
	`req_lawful`	        INTEGER NOT NULL DEFAULT 0,
	`req_neutral`	        INTEGER NOT NULL DEFAULT 0,
	`req_chaotic`	        INTEGER NOT NULL DEFAULT 0,
	`c40`	                INTEGER,
	`c41`	                INTEGER,
	`req_str`	            INTEGER NOT NULL DEFAULT 0,
	`req_vit`	            INTEGER NOT NULL DEFAULT 0,
	`req_dex`	            INTEGER NOT NULL DEFAULT 0,
	`req_agi`	            INTEGER NOT NULL DEFAULT 0,
	`req_int`	            INTEGER NOT NULL DEFAULT 0,
	`req_pie`	            INTEGER NOT NULL DEFAULT 0,
	`req_luk`	            INTEGER NOT NULL DEFAULT 0,
	`req_soul_rank`	        INTEGER NOT NULL DEFAULT 0,
    `max_soul_rank`	        INTEGER NOT NULL DEFAULT 0,
	`req_lvl`	            INTEGER NOT NULL DEFAULT 0,
	`c51`	                TEXT,
	`c52`	                TEXT,
    `c53`	                TEXT,
	`phys_slash`	        INTEGER NOT NULL DEFAULT 0,
	`phys_strike`	        INTEGER NOT NULL DEFAULT 0,
	`phys_pierce`	        INTEGER NOT NULL DEFAULT 0,
	`c56`	                TEXT,
	`pdef_fire`	            INTEGER NOT NULL DEFAULT 0,
	`pdef_water`	        INTEGER NOT NULL DEFAULT 0,
	`pdef_wind`	            INTEGER NOT NULL DEFAULT 0,
	`pdef_earth`	        INTEGER NOT NULL DEFAULT 0,
	`pdef_light`	        INTEGER NOT NULL DEFAULT 0,
	`pdef_dark`	            INTEGER NOT NULL DEFAULT 0,
	`c63`	                TEXT,
	`c64`	                TEXT,
	`c65`	                TEXT,
	`matk_fire`	            INTEGER NOT NULL DEFAULT 0,
	`matk_water`	        INTEGER NOT NULL DEFAULT 0,
	`matk_wind`	            INTEGER NOT NULL DEFAULT 0,
	`matk_earth`	        INTEGER NOT NULL DEFAULT 0,
	`matk_light`	        INTEGER NOT NULL DEFAULT 0,
	`matk_dark`	            INTEGER NOT NULL DEFAULT 0,
	`c72`	                TEXT,
	`c73`	                TEXT,
	`c74`	                TEXT,
	`c75`	                TEXT,
	`c76`	                TEXT,
	`seffect_hp`	        INTEGER NOT NULL DEFAULT 0,
	`seffect_mp`	        INTEGER NOT NULL DEFAULT 0,
	`seffect_str`	        INTEGER NOT NULL DEFAULT 0,
	`seffect_vit`	        INTEGER NOT NULL DEFAULT 0,
	`seffect_dex`	        INTEGER NOT NULL DEFAULT 0,
	`seffect_agi`	        INTEGER NOT NULL DEFAULT 0,
	`seffect_int`	        INTEGER NOT NULL DEFAULT 0,
	`seffect_pie`	        INTEGER NOT NULL DEFAULT 0,
	`seffect_luk`	        INTEGER NOT NULL DEFAULT 0,
	`res_poison`	        INTEGER NOT NULL DEFAULT 0,
	`res_paralyze`	        INTEGER NOT NULL DEFAULT 0,
	`res_petrified`	        INTEGER NOT NULL DEFAULT 0,
	`res_faint`	            INTEGER NOT NULL DEFAULT 0,
	`res_blind`	            INTEGER NOT NULL DEFAULT 0,
	`res_sleep`	            INTEGER NOT NULL DEFAULT 0,
	`res_silent`	        INTEGER NOT NULL DEFAULT 0,
	`res_charm`	            INTEGER NOT NULL DEFAULT 0,
	`res_confusion`	        INTEGER NOT NULL DEFAULT 0,
	`res_fear`	            INTEGER NOT NULL DEFAULT 0,
	`c96`	                TEXT,
	`status_malus`	        TEXT,
	`status_percent`	    INTEGER,
	`num_of_bag_slots`	    INTEGER,
	`object_type`	        TEXT NOT NULL DEFAULT `NONE`,
	`equip_slot`	        TEXT,
	`c102`	                TEXT,
	`c103`	                TEXT,
	`c104`	                TEXT,
	`no_use_in_town`	    INTEGER NOT NULL DEFAULT 0,
	`no_storage`	        INTEGER NOT NULL DEFAULT 0,
	`no_discard`	        INTEGER NOT NULL DEFAULT 0,
	`no_sell`	            INTEGER NOT NULL DEFAULT 0,
	`no_trade`	            INTEGER NOT NULL DEFAULT 0,
	`no_trade_after_used`	INTEGER NOT NULL DEFAULT 0,
	`no_stolen`	            INTEGER NOT NULL DEFAULT 0,
	`gold_border`	        INTEGER NOT NULL DEFAULT 0,
	`lore`	                TEXT,
	`icon`	                INTEGER NOT NULL DEFAULT 0,
	`field118`	            TEXT,
	`field119`	            TEXT,
	`field120`	            TEXT,
	`field121`	            TEXT,
	`field122`	            TEXT,
	`field123`	            TEXT,
	`field124`	            TEXT,
	`field125`	            TEXT,
	`field126`	            TEXT,
	`field127`	            TEXT,
	`field128`	            TEXT,
	`field129`	            TEXT,
	`field130`	            TEXT,
	`req_samurai`	        INTEGER NOT NULL DEFAULT 0,
	`req_ninja`	            INTEGER NOT NULL DEFAULT 0,
	`req_bishop`	        INTEGER NOT NULL DEFAULT 0,
	`req_lord`	            INTEGER NOT NULL DEFAULT 0,
	`field135`	            TEXT,
	`field136`	            TEXT,
	`field137`	            TEXT,
	`field138`	            TEXT,
	`field139`	            TEXT,
    `field140`	            TEXT,
	`req_clown`	            INTEGER NOT NULL DEFAULT 0,
	`req_alchemist`	        INTEGER NOT NULL DEFAULT 0,
	`grade`	                INTEGER NOT NULL DEFAULT 0,
	`hardness`	            INTEGER NOT NULL DEFAULT 0,
	`scroll_id`	            INTEGER NOT NULL DEFAULT 0,  
	`physical`	            INTEGER NOT NULL DEFAULT 0,
	`magical`	            INTEGER NOT NULL DEFAULT 0,
	`weight`	            INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS `nec_item_instance` (
	`id`	INTEGER NOT NULL,
	`owner_id`	INTEGER NOT NULL,
	`zone`	INTEGER NOT NULL,
	`container`	INTEGER NOT NULL,
	`slot`	INTEGER NOT NULL,
	`base_id`	INTEGER NOT NULL,
	`quantity`	INTEGER NOT NULL DEFAULT 1,
	`statuses`	INTEGER NOT NULL DEFAULT 0,
	`current_equip_slot`	INTEGER NOT NULL DEFAULT 0,
	`current_durability`	INTEGER NOT NULL DEFAULT 0,
	`plus_maximum_durability`	INTEGER NOT NULL DEFAULT 0,
	`enhancement_level`	INTEGER NOT NULL DEFAULT 0,
	`special_forge_level`	INTEGER NOT NULL DEFAULT 0,
	`plus_physical`	INTEGER NOT NULL DEFAULT 0,
	`plus_magical`	INTEGER NOT NULL DEFAULT 0,
	`plus_hardness`	INTEGER NOT NULL DEFAULT 0,
	`gem_slot_1_type`	INTEGER NOT NULL DEFAULT 0,
	`gem_slot_2_type`	INTEGER NOT NULL DEFAULT 0,
	`gem_slot_3_type`	INTEGER NOT NULL DEFAULT 0,
	`gem_id_slot_1`	INTEGER,
	`gem_id_slot_2`	INTEGER,
	`gem_id_slot_3`	INTEGER,
	`enchant_id`	INTEGER NOT NULL DEFAULT 0,
	`plus_gp`	INTEGER NOT NULL DEFAULT 0,
	`plus_weight`	INTEGER NOT NULL DEFAULT (0),
	`plus_ranged_eff`	INTEGER NOT NULL DEFAULT (0),
	`plus_reservoir_eff`	INTEGER NOT NULL DEFAULT (0),
    `consigner_soul_name`  TEXT,
    `winner_soul_id`    INTEGER,
	`expiry_datetime`	INTEGER,
	`min_bid`			INTEGER,
	`buyout_price`		INTEGER,
	`comment`			TEXT,
	PRIMARY KEY(`id` AUTOINCREMENT),
	FOREIGN KEY(`owner_id`) REFERENCES `nec_character`(`id`) ON DELETE CASCADE,
	FOREIGN KEY(`base_id`) REFERENCES `nec_item_library`(`id`) ON UPDATE CASCADE ON DELETE RESTRICT
    FOREIGN KEY(`winner_soul_id`) REFERENCES `nec_soul`(`id`) ON UPDATE CASCADE ON DELETE SET NULL
);

CREATE VIEW IF NOT EXISTS item_instance
    AS
 SELECT             
                    nec_item_instance.id,
                    owner_id,
                    zone,
                    container,
                    slot,
                    base_id,
                    quantity,
                    statuses,
                    current_equip_slot,
                    current_durability,
                    plus_maximum_durability,
                    enhancement_level,
                    special_forge_level,
                    nec_item_library.physical,
                    nec_item_library.magical,
                    nec_item_library.hardness,
					gem_slot_1_type,
                    gem_slot_2_type,
                    gem_slot_3_type,
                    gem_id_slot_1,
                    gem_id_slot_2,
                    gem_id_slot_3,
                    enchant_id,
					item_type,
                    quality,
                    _item_name_necromancy,
                    max_stack_size,
                    "3",
                    es_hand_r,
                    es_hand_l,
                    es_quiver,
                    es_head,
                    es_body,
                    es_legs,
                    es_arms,
                    es_feet,
                    es_mantle,
                    es_ring,
                    es_earring,
                    es_necklace,
                    es_belt,
                    es_talkring,
                    es_avatar_head,
                    es_avatar_body,
                    es_avatar_legs,
                    es_avatar_arms,
                    es_avatar_feet,
                    es_avatar_mantle,
                    req_hum_m,
                    req_hum_f,
                    req_elf_m,
                    req_elf_f,
                    req_dwf_m,
                    req_dwf_f,
                    req_por_m,
                    req_por_f,
                    req_gnm_m,
                    req_gnm_f,
                    req_fighter,
                    req_thief,
                    req_mage,
                    req_priest,
                    req_lawful,
                    req_neutral,
                    req_chaotic,
                    "40",
                    "41",
                    req_str,
                    req_vit,
                    req_dex,
                    req_agi,
                    req_int,
                    req_pie,
                    req_luk,
                    req_soul_rank,
                    req_lvl,
					"51",
                    "52",
                    "53",
                    phys_slash,
                    phys_strike,
                    phys_pierce,
                    "56",
                    pdef_fire,
                    pdef_water,
                    pdef_wind,
                    pdef_earth,
                    pdef_light,
                    pdef_dark,
                    "63",
                    "64",
                    "65",
                    matk_fire,
                    matk_water,
                    matk_wind,
                    matk_earth,
                    matk_light,
                    matk_dark,
                    "72",
                    "73",
                    "74",
                    "75",
                    "76",
                    seffect_hp,
                    seffect_mp,
                    seffect_str,
                    seffect_vit,
                    seffect_dex,
                    seffect_agi,
                    seffect_int,
                    seffect_pie,
                    seffect_luk,
                    res_poison,
                    res_paralyze,
                    res_petrified,
                    res_faint,
                    res_blind,
                    res_sleep,
                    res_silent,
                    res_charm,
                    res_confusion,
                    res_fear,
                    "96",
                    status_malus,
                    status_percent,
                    num_of_bag_slots,
                    object_type,
                    equip_slot,
                    "102",
                    "103",
                    "104",
                    no_use_in_town,
                    no_storage,
                    no_discard,
                    no_sell,
                    no_trade,
                    no_trade_after_used,
                    no_stolen,
                    gold_border,
                    lore,
                    icon,
                    field118,
                    field119,
                    field120,
                    field121,
                    field122,
                    field123,
                    field124,
                    field125,
                    field126,
                    field127,
                    field128,
                    field129,
                    field130,
                    req_samurai,
                    req_ninja,
                    req_bishop,
                    req_lord,
                    field135,
                    field136,
                    field137,
                    field138,
                    field139,
                    req_clown,
                    req_alchemist,
                    grade,
                    nec_item_library.hardness,
                    scroll_id,
                    weight,
                    plus_physical,
                    plus_magical,
                    plus_hardness,
                    plus_gp,
                    plus_weight,
                    plus_ranged_eff,
                    plus_reservoir_eff,
                    consigner_soul_name,
                    winner_soul_id,
	                expiry_datetime,
	                min_bid,
	                buyout_price,
	                comment
                FROM 
                    nec_item_instance 
                INNER JOIN 
                    nec_item_library 
                ON 
                    nec_item_instance.base_id = nec_item_library.id;

 CREATE TABLE IF NOT EXISTS "nec_auction_bids" (
	"item_instance_id"	INTEGER NOT NULL,
	"bidder_soul_id"	INTEGER NOT NULL,
	"current_bid"	INTEGER,
	PRIMARY KEY("item_instance_id","bidder_soul_id"),
    FOREIGN KEY(`item_instance_id`) REFERENCES `nec_item_instance`(`id`) ON UPDATE CASCADE ON DELETE CASCADE,
    FOREIGN KEY(`bidder_soul_id`) REFERENCES `nec_soul`(`id`) ON UPDATE CASCADE ON DELETE CASCADE
)


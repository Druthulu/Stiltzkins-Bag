using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;

namespace rand9er
{
    public static class Bytes
    {
        public static void Bytes_IO()
        {






            // Refactor because we wont need backups

            // We might be scrapping this whole funtion


            if (!File.Exists(rand9er.binloc + "\\p0data7.bak"))
            { File.Copy(rand9er.binloc + "\\p0data7.bin", rand9er.binloc + "\\p0data7.bak"); }
            if (!File.Exists(rand9er.binloc + "\\p0data2.bak"))
            { File.Copy(rand9er.binloc + "\\p0data2.bin", rand9er.binloc + "\\p0data2.bak"); }
            rand9er.ba_p0data7 = File.ReadAllBytes(rand9er.binloc + "\\p0data7.bin");
            //ba_p2 = File.ReadAllBytes(binloc + "\\p0data2.bin");

            Dictionary<int, string> ff9Fields = new Dictionary<int, string>
        {
            { 50, "EVT_ALEX1_TS_CARGO_0" }, { 51, "EVT_ALEX1_TS_CARGO_3" }, { 52, "EVT_ALEX1_TS_MEET_0" }, { 53, "EVT_ALEX1_TS_MEET_1" }, { 54, "EVT_ALEX1_TS_UPPER" }, { 55, "EVT_ALEX1_TS_ORCHE" }, { 56, "EVT_ALEX1_TS_ENGIN_UP" }, { 57, "EVT_ALEX1_TS_ENGIN" }, { 58, "EVT_ALEX1_TS_STORAGE" }, { 59, "EVT_ALEX1_TS_UNDER" },
            { 60, "EVT_ALEX1_TS_COCKPIT" }, { 61, "EVT_ALEX1_TS_STAGE_BK" }, { 62, "EVT_ALEX1_TS_STAGE_1A" }, { 63, "EVT_ALEX1_TS_STAGE_1B" }, { 64, "EVT_ALEX1_TS_SWD_BTL" }, { 65, "EVT_ALEX1_TS_STAGE_2A" }, { 66, "EVT_ALEX1_TS_STAGE_2B" }, { 67, "EVT_ALEX1_TS_STAGE_2C" }, { 68, "EVT_ALEX1_TS_ROYAL_1" }, { 69, "EVT_ALEX1_TS_ROYAL_2" },
            { 70, "EVT_ALEX1_TS_OPENING" }, { 100, "EVT_ALEX1_AT_STREET_A" }, { 101, "EVT_ALEX1_AT_STREET_B" }, { 102, "EVT_ALEX1_AT_STREET_C" }, { 103, "EVT_ALEX1_AT_CENTER" }, { 104, "EVT_ALEX1_AT_TICKET" }, { 105, "EVT_ALEX1_AT_BACK_STR" }, { 106, "EVT_ALEX1_AT_TSS" }, { 107, "EVT_ALEX1_AT_GATE" }, { 108, "EVT_ALEX1_AT_ITEM" },
            { 109, "EVT_ALEX1_AT_WEAPON" }, { 110, "EVT_ALEX1_AT_MAGIC" }, { 111, "EVT_ALEX1_AT_INN" }, { 112, "EVT_ALEX1_AT_PUB" }, { 113, "EVT_ALEX1_AT_HOUSE_1" }, { 114, "EVT_ALEX1_AT_HOUSE_2" }, { 115, "EVT_ALEX1_AT_SENTOU" }, { 116, "EVT_ALEX1_AT_ROOF" }, { 117, "EVT_ALEX1_AT_RUBY" }, { 150, "EVT_ALEX1_AC_GUARD" },
            { 151, "EVT_ALEX1_AC_SEAT_R" }, { 153, "EVT_ALEX1_AC_H2F" }, { 154, "EVT_ALEX1_AC_ENT_2F" }, { 155, "EVT_ALEX1_AC_LIB" }, { 156, "EVT_ALEX1_AC_PARTY" }, { 157, "EVT_ALEX1_AC_KITCHEN" }, { 158, "EVT_ALEX1_AC_OUT_E" }, { 159, "EVT_ALEX1_AC_OUT_C" }, { 160, "EVT_ALEX1_AC_OUT_L" }, { 161, "EVT_ALEX1_AC_OUT_R" },
            { 162, "EVT_ALEX1_AC_TOWER_L1" }, { 163, "EVT_ALEX1_AC_TOWER_L2" }, { 164, "EVT_ALEX1_AC_TOWER_L3" }, { 165, "EVT_ALEX1_AC_TOWER_L4" }, { 166, "EVT_ALEX1_AC_TOWER_L5" }, { 167, "EVT_ALEX1_AC_LIB_2" }, { 200, "EVT_DOWNSHIP_BT_CCD_0" }, { 201, "EVT_DOWNSHIP_BT_CCP_0" }, { 202, "EVT_DOWNSHIP_BT_CCR_0" },
            { 203, "EVT_DOWNSHIP_BT_CMR_0" }, { 204, "EVT_DOWNSHIP_BT_CNR_0" }, { 205, "EVT_DOWNSHIP_BT_CUS_0" }, { 206, "EVT_DOWNSHIP_BT_FRT_0" }, { 207, "EVT_DOWNSHIP_BT_GTR_0" }, { 208, "EVT_DOWNSHIP_BT_STR_0" }, { 209, "EVT_DOWNSHIP_BT_CF1_0" }, { 250, "EVT_EFOREST1_EF_ENT_0" }, { 251, "EVT_EFOREST1_EF_FR1_0" },
            { 252, "EVT_EFOREST1_EF_FR2_0" }, { 253, "EVT_EFOREST1_EF_HEL_0" }, { 254, "EVT_EFOREST1_EF_NSP_0" }, { 255, "EVT_EFOREST1_EF_NRV_0" }, { 256, "EVT_EFOREST1_EF_FR3_0" }, { 257, "EVT_EFOREST1_EF_DEP_0" }, { 258, "EVT_EFOREST1_EF_FR4_0" }, { 259, "EVT_EFOREST1_EF_FR5_0" }, { 260, "EVT_EFOREST1_EF_FR6_0" },
            { 261, "EVT_EFOREST1_EF_EXT_0" }, { 262, "EVT_EFOREST1_EF_TNT_0" }, { 300, "EVT_ICE_IC_ENT_0" }, { 301, "EVT_ICE_IC_STP_0" }, { 302, "EVT_ICE_IC_TER_0" }, { 303, "EVT_ICE_IC_JMP_0" }, { 304, "EVT_ICE_IC_BRI_0" }, { 305, "EVT_ICE_IC_MEN_0" }, { 306, "EVT_ICE_IC_MRM_0" }, { 307, "EVT_ICE_IC_STA_0" },
            { 308, "EVT_ICE_IC_WBF_0" }, { 309, "EVT_ICE_IC_WAF_0" }, { 310, "EVT_ICE_IC_CAF_0" }, { 311, "EVT_ICE_IC_XIT_0" }, { 312, "EVT_ICE_DL_VIW_0" }, { 350, "EVT_DALI_V_DL_WHL" }, { 351, "EVT_DALI_V_DL_INN0" }, { 352, "EVT_DALI_V_DL_INN1" }, { 353, "EVT_DALI_V_DL_MYR" }, { 354, "EVT_DALI_V_DL_SHP" },
            { 355, "EVT_DALI_V_DL_BAR" }, { 356, "EVT_DALI_V_DL_WMS0" }, { 357, "EVT_DALI_V_DL_WMS1" }, { 358, "EVT_DALI_V_DL_FWM" }, { 359, "EVT_DALI_V_DL_ENT" }, { 400, "EVT_DALI_F_UF_BLK_0" }, { 401, "EVT_DALI_F_UF_BLK_1" }, { 402, "EVT_DALI_F_UF_DEV_0" }, { 403, "EVT_DALI_F_UF_EGG_0" }, { 404, "EVT_DALI_F_UF_ENT_0" },
            { 405, "EVT_DALI_F_UF_MST_0" }, { 406, "EVT_DALI_F_UF_RST_0" }, { 407, "EVT_DALI_F_UF_STO_0" }, { 408, "EVT_DALI_F_UF_STO_1" }, { 450, "EVT_DALI_A_AP_FID_0" }, { 451, "EVT_DALI_A_AP_AIR_0" }, { 452, "EVT_DALI_A_AP_AIR_1" }, { 453, "EVT_DALI_A_AP_AIR_2" }, { 454, "EVT_DALI_A_AP_AIR_3" }, { 455, "EVT_DALI_A_AP_WTE_0" },
            { 456, "EVT_DALI_A_AP_WTE_1" }, { 457, "EVT_DALI_A_AP_WTI_0" }, { 500, "EVT_CARGO_CA_DCK_1" }, { 501, "EVT_CARGO_CA_DCK_W" }, { 502, "EVT_CARGO_CA_COP_0" }, { 503, "EVT_CARGO_CA_COP_1" }, { 504, "EVT_CARGO_CA_ERM_0" }, { 505, "EVT_CARGO_CA_RDK_0" }, { 506, "EVT_CARGO_FN_DCK_0" }, { 507, "EVT_CARGO_CA_DCK_0" },
            { 550, "EVT_LIND1_TN_LB_GAT_0" }, { 551, "EVT_LIND1_TN_LB_MTM_0" }, { 552, "EVT_LIND1_TN_LB_MST_0" }, { 553, "EVT_LIND1_TN_LB_IN1_0" }, { 554, "EVT_LIND1_TN_LB_IN2_0" }, { 555, "EVT_LIND1_TN_LB_BAZ_0" }, { 556, "EVT_LIND1_TN_LB_ALY_0" }, { 557, "EVT_LIND1_TN_LB_TMP_0" }, { 558, "EVT_LIND1_TN_LB_HUS_0" },
            { 559, "EVT_LIND1_TN_LB_PLZ_0" }, { 560, "EVT_LIND1_TN_LB_ECH_0" }, { 561, "EVT_LIND1_TN_LB_ITM_0" }, { 562, "EVT_LIND1_TN_LB_WPN_0" }, { 563, "EVT_LIND1_TN_LB_STN_0" }, { 564, "EVT_LIND1_TN_LB_ATL_0" }, { 565, "EVT_LIND1_TN_LB_TTL_0" }, { 566, "EVT_LIND1_TN_LB_RFS_0" }, { 567, "EVT_LIND1_TN_LB_HIG_0" },
            { 568, "EVT_LIND1_TN_LB_FTH_0" }, { 569, "EVT_LIND1_TN_LB_AET_0" }, { 570, "EVT_LIND1_TN_LB_ROD_0" }, { 571, "EVT_LIND1_TN_LB_SLN_0" }, { 572, "EVT_LIND1_TN_LB_FTM_0" }, { 573, "EVT_LIND1_TN_LB_LDH_0" }, { 574, "EVT_LIND1_TN_LB_WGN_0" }, { 575, "EVT_LIND1_TN_LB_CAG_0" }, { 576, "EVT_LIND1_TN_LB_SHT_0" },
            { 600, "EVT_LIND1_CS_LB_CFR_0" }, { 601, "EVT_LIND1_CS_LB_LPF_0" }, { 602, "EVT_LIND1_CS_LB_EXB_0" }, { 603, "EVT_LIND1_CS_LB_LLP_0" }, { 604, "EVT_LIND1_CS_LB_POT_0" }, { 605, "EVT_LIND1_CS_LB_PLA_0" }, { 606, "EVT_LIND1_CS_LB_TSP_0" }, { 607, "EVT_LIND1_CS_LB_HNG_0" }, { 608, "EVT_LIND1_CS_LB_SHB_0" },
            { 609, "EVT_LIND1_CS_LB_BRG_0" }, { 610, "EVT_LIND1_CS_LB_CTM_0" }, { 611, "EVT_LIND1_CS_LB_GRM_0" }, { 612, "EVT_LIND1_CS_LB_MHW_0" }, { 613, "EVT_LIND1_CS_LB_HAL_0" }, { 614, "EVT_LIND1_CS_LB_ASD_0" }, { 615, "EVT_LIND1_CS_LB_OBS_0" }, { 616, "EVT_LIND1_CS_LB_LPR_0" }, { 617, "EVT_LIND1_CS_LB_UHW_0" },
            { 618, "EVT_LIND1_CS_LB_MET_0" }, { 619, "EVT_LIND1_CS_LB_HFC_0" }, { 620, "EVT_LIND1_CS_LB_LFT_0" }, { 650, "EVT_KUINA_KM_ENT_0" }, { 651, "EVT_KUINA_KM_DWN_0" }, { 652, "EVT_KUINA_KM_DWN_1" }, { 653, "EVT_KUINA_KM_DWN_2" }, { 654, "EVT_KUINA_KM_DWN_3" }, { 655, "EVT_KUINA_KM_RED_0" }, { 657, "EVT_KUINA_KM_SWP_1" },
            { 660, "EVT_KUINA_KM_THE_0" }, { 661, "EVT_KUINA_KM_THI_0" }, { 701, "EVT_GIZ_ENTER_1" }, { 702, "EVT_GIZ_CAVERN_0" }, { 703, "EVT_GIZ_CAVERN_1" }, { 704, "EVT_GIZ_BELL_0" }, { 705, "EVT_GIZ_BELL_1" }, { 706, "EVT_GIZ_TO_WORLD" }, { 707, "EVT_GIZ_BOSS" }, { 750, "EVT_BURMECIA_ENTER" }, { 751, "EVT_BURMECIA_TOWN_0" }, { 752, "EVT_BURMECIA_TOWN_1" },
            { 753, "EVT_BURMECIA_TOWN_2" }, { 754, "EVT_BURMECIA_HOUSE_0" }, { 755, "EVT_BURMECIA_HOUSE_1" }, { 756, "EVT_BURMECIA_HOUSE_2" }, { 757, "EVT_BURMECIA_HOUSE_3" }, { 758, "EVT_BURMECIA_PATH" }, { 759, "EVT_BURMECIA_PATIO_0" }, { 760, "EVT_BURMECIA_PATIO_1" }, { 761, "EVT_BURMECIA_PATIO_2" }, { 762, "EVT_BURMECIA_PATIO_3" },
            { 763, "EVT_BURMECIA_SQUARE_0" }, { 764, "EVT_BURMECIA_SQUARE_1" }, { 765, "EVT_BURMECIA_SQUARE_2" }, { 766, "EVT_BURMECIA_PALACE_0" }, { 767, "EVT_BURMECIA_PALACE_1" }, { 768, "EVT_BURMECIA_PALACE_2" }, { 800, "EVT_GATE_S_SG_RND_0" }, { 801, "EVT_GATE_S_SG_RND_1" }, { 802, "EVT_GATE_S_SG_RND_3" }, { 806, "EVT_GATE_S_SG_ALX_3" },
            { 813, "EVT_GATE_S_SG_TRN_0" }, { 814, "EVT_GATE_S_SG_TRN_1" }, { 850, "EVT_GATE_N_NG_BDA_0" }, { 851, "EVT_GATE_N_NG_BMA_0" }, { 852, "EVT_GATE_N_NG_MDA_0" }, { 1256, "EVT_PINA_PR_ENT_1" }, { 2950, "EVT_CHOCO_CH_FST_0" }, { 2953, "EVT_CHOCO_CH_DWL_0" }, { 152, "EVT_ALEX1_AC_SEAT_N" }, { 656, "EVT_KUINA_KM_SWP_0" },
            { 658, "EVT_KUINA_KM_SWP_2" }, { 659, "EVT_KUINA_KM_SWP_3" }, { 662, "EVT_KUINA_KM_FET_0" }, { 663, "EVT_KUINA_KM_FET_1" }, { 769, "EVT_BURMECIA_PALACE_3" }, { 803, "EVT_GATE_S_SG_ALX_0" }, { 804, "EVT_GATE_S_SG_ALX_1" }, { 805, "EVT_GATE_S_SG_ALX_2" }, { 807, "EVT_GATE_S_SG_ALX_4" }, { 809, "EVT_GATE_S_SG_TOP_0" },
            { 810, "EVT_GATE_S_SG_TOP_1" }, { 811, "EVT_GATE_S_SG_TOP_2" }, { 812, "EVT_GATE_S_SG_TOP_3" }, { 815, "EVT_GATE_S_SG_TRN_2" }, { 816, "EVT_GATE_S_SG_TRN_3" }, { 900, "EVT_TRENO1_TR_BAR_0" }, { 901, "EVT_TRENO1_TR_BHE_0" }, { 902, "EVT_TRENO1_TR_BHM_0" }, { 903, "EVT_TRENO1_TR_BTA_0" }, { 904, "EVT_TRENO1_TR_EKH_0" },
            { 905, "EVT_TRENO1_TR_FBH_0" }, { 906, "EVT_TRENO1_TR_FKH_0" }, { 907, "EVT_TRENO1_TR_FQH_0" }, { 908, "EVT_TRENO1_TR_GAT_0" }, { 909, "EVT_TRENO1_TR_KHA_0" }, { 910, "EVT_TRENO1_TR_KHM_0" }, { 911, "EVT_TRENO1_TR_QHM_0" }, { 912, "EVT_TRENO1_TR_RES_0" }, { 913, "EVT_TRENO1_TR_TMS_0" }, { 914, "EVT_TRENO1_TR_TOT_0" },
            { 915, "EVT_TRENO1_TR_WBH_0" }, { 916, "EVT_TRENO1_TR_WHF_0" }, { 930, "EVT_TOT_TR_TOT_1" }, { 931, "EVT_TOT_TR_WAT_0" }, { 932, "EVT_TOT_AC_LBR_A" }, { 950, "EVT_GARGAN_GR_CEN_0" }, { 951, "EVT_GARGAN_GR_LEF_0" }, { 952, "EVT_GARGAN_GR_GBA_0" }, { 953, "EVT_GARGAN_GR_REN_0" }, { 954, "EVT_GARGAN_GR_GGT_0" },
            { 955, "EVT_GARGAN_GR_GGT_1" }, { 956, "EVT_GARGAN_GR_ALX_0" }, { 957, "EVT_GARGAN_GR_CON_0" }, { 1000, "EVT_CLEYRA1_ENT_0" }, { 1001, "EVT_CLEYRA1_ENT_1" }, { 1002, "EVT_CLEYRA1_ENT_2" }, { 1003, "EVT_CLEYRA1_DUN_00" }, { 1004, "EVT_CLEYRA1_DUN_01A" }, { 1005, "EVT_CLEYRA1_DUN_01B" }, { 1006, "EVT_CLEYRA1_DUN_03" },
            { 1007, "EVT_CLEYRA1_DUN_04" }, { 1008, "EVT_CLEYRA1_DUN_05" }, { 1009, "EVT_CLEYRA1_DUN_06" }, { 1010, "EVT_CLEYRA1_DUN_07" }, { 1011, "EVT_CLEYRA1_DUN_08" }, { 1012, "EVT_CLEYRA1_DUN_09" }, { 1013, "EVT_CLEYRA1_DUN_10" }, { 1014, "EVT_CLEYRA1_DUN_11" }, { 1015, "EVT_CLEYRA1_DUN_12A" }, { 1016, "EVT_CLEYRA1_DUN_12B" },
            { 1017, "EVT_CLEYRA1_DUN_13A" }, { 1018, "EVT_CLEYRA1_DUN_13B" }, { 1050, "EVT_CLEYRA2_LADDER" }, { 1051, "EVT_CLEYRA2_ENTER" }, { 1052, "EVT_CLEYRA2_ANTRION" }, { 1053, "EVT_CLEYRA2_WATER" }, { 1054, "EVT_CLEYRA2_WIND" }, { 1055, "EVT_CLEYRA2_TOWN" }, { 1056, "EVT_CLEYRA2_INN" }, { 1057, "EVT_CLEYRA2_VISTA" },
            { 1058, "EVT_CLEYRA2_CATHE_0" }, { 1059, "EVT_CLEYRA2_CATHE_1" }, { 1060, "EVT_CLEYRA2_CATHE_2" }, { 1100, "EVT_CLEYRA3_LADDER" }, { 1101, "EVT_CLEYRA3_ENTER" }, { 1102, "EVT_CLEYRA3_ANTRION" }, { 1103, "EVT_CLEYRA3_WATER" }, { 1104, "EVT_CLEYRA3_WIND" }, { 1105, "EVT_CLEYRA3_TOWN" }, { 1106, "EVT_CLEYRA3_INN" },
            { 1107, "EVT_CLEYRA3_VISTA" }, { 1108, "EVT_CLEYRA3_CATHE_0" }, { 1109, "EVT_CLEYRA3_CATHE_1" }, { 1110, "EVT_CLEYRA3_CATHE_2" }, { 1150, "EVT_ROSE_DECK" }, { 1151, "EVT_ROSE_CABIN_1" }, { 1152, "EVT_ROSE_CABIN_2" }, { 1153, "EVT_ROSE_BRIDGE" }, { 1200, "EVT_ALEX2_AC_SEAT_R" }, { 1201, "EVT_ALEX2_AC_S_ROOM" },
            { 1202, "EVT_ALEX2_AC_STAIR_A" }, { 1203, "EVT_ALEX2_AC_STAIR_B" }, { 1204, "EVT_ALEX2_AC_STAIR_C" }, { 1205, "EVT_ALEX2_AC_SUMMON_A" }, { 1206, "EVT_ALEX2_AC_QUEEN" }, { 1207, "EVT_ALEX2_AC_PRINCESS" }, { 1208, "EVT_ALEX2_AC_PRISON_A" }, { 1209, "EVT_ALEX2_AC_PRISON_B" }, { 1210, "EVT_ALEX2_AC_TOWER_L1" },
            { 1211, "EVT_ALEX2_AC_TOWER_R1" }, { 1212, "EVT_ALEX2_AC_TOWER_R2" }, { 1213, "EVT_ALEX2_AC_GUARD" }, { 1214, "EVT_ALEX2_AC_H2F" }, { 1215, "EVT_ALEX2_AC_ENT_2F" }, { 1216, "EVT_ALEX2_AC_LIB" }, { 1217, "EVT_ALEX2_AC_PARTY" }, { 1218, "EVT_ALEX2_AC_KITCHEN" }, { 1219, "EVT_ALEX2_AC_OUT_E" }, { 1220, "EVT_ALEX2_AC_OUT_C" },
            { 1221, "EVT_ALEX2_AC_OUT_L" }, { 1222, "EVT_ALEX2_AC_OUT_R" }, { 1223, "EVT_ALEX2_AC_QUEEN_2" }, { 1224, "EVT_ALEX2_AC_R_HALL" }, { 1225, "EVT_ALEX2_AC_QUEEN_3" }, { 1226, "EVT_ALEX2_AC_LIB_2" }, { 1227, "EVT_ALEX2_AC_SWD_BTL" }, { 1250, "EVT_PINA_AC_RST_0" }, { 1251, "EVT_PINA_PR_GAL_0" }, { 1252, "EVT_PINA_PR_PW1_0" },
            { 1253, "EVT_PINA_PR_PW2_0" }, { 1254, "EVT_PINA_PR_PW3_0" }, { 1255, "EVT_PINA_PR_ENT_0" }, { 1300, "EVT_LIND2_TN_LB_GAT_0" }, { 1301, "EVT_LIND2_TN_LB_MTM_0" }, { 1302, "EVT_LIND2_TN_LB_MST_0" }, { 1303, "EVT_LIND2_TN_LB_IN1_0" }, { 1304, "EVT_LIND2_TN_LB_IN2_0" }, { 1305, "EVT_LIND2_TN_LB_BAZ_0" },
            { 1306, "EVT_LIND2_TN_LB_HUS_0" }, { 1307, "EVT_LIND2_TN_LB_PLZ_0" }, { 1308, "EVT_LIND2_TN_LB_ECH_0" }, { 1309, "EVT_LIND2_TN_LB_WPN_0" }, { 1310, "EVT_LIND2_TN_LB_STN_0" }, { 1311, "EVT_LIND2_TN_LB_ATL_0" }, { 1312, "EVT_LIND2_TN_LB_TTL_0" }, { 1313, "EVT_LIND2_TN_LB_RFS_0" }, { 1314, "EVT_LIND2_TN_LB_HIG_0" },
            { 1315, "EVT_LIND2_TN_LB_OMG_W" }, { 1350, "EVT_LIND2_CS_LB_CFR_0" }, { 1351, "EVT_LIND2_CS_LB_LPF_0" }, { 1352, "EVT_LIND2_CS_LB_EXB_0" }, { 1353, "EVT_LIND2_CS_LB_LLP_0" }, { 1354, "EVT_LIND2_CS_LB_POT_0" }, { 1355, "EVT_LIND2_CS_LB_PLA_0" }, { 1356, "EVT_LIND2_CS_LB_TSP_0" }, { 1357, "EVT_LIND2_CS_LB_HNG_0" },
            { 1358, "EVT_LIND2_CS_LB_SHB_0" }, { 1359, "EVT_LIND2_CS_LB_BRG_0" }, { 1360, "EVT_LIND2_CS_LB_CTM_0" }, { 1361, "EVT_LIND2_CS_LB_GRM_0" }, { 1362, "EVT_LIND2_CS_LB_MHW_0" }, { 1363, "EVT_LIND2_CS_LB_HAL_0" }, { 1364, "EVT_LIND2_CS_LB_ASD_1" }, { 1365, "EVT_LIND2_CS_LB_OBS_0" }, { 1366, "EVT_LIND2_CS_LB_LPR_0" },
            { 1367, "EVT_LIND2_CS_LB_UHW_0" }, { 1368, "EVT_LIND2_CS_LB_MET_0" }, { 1369, "EVT_LIND2_CS_LB_HFC_0" }, { 1370, "EVT_LIND2_CS_LB_LFT_0" }, { 1400, "EVT_FOSSIL_FR_MV1_0" }, { 1401, "EVT_FOSSIL_FR_MV2_0" }, { 1402, "EVT_FOSSIL_FR_MV3_0" }, { 1403, "EVT_FOSSIL_FR_MV4_0" }, { 1404, "EVT_FOSSIL_FR_MV5_0" },
            { 1405, "EVT_FOSSIL_FR_SL1_0" }, { 1406, "EVT_FOSSIL_FR_SL2_0" }, { 1407, "EVT_FOSSIL_FR_SL3_0" }, { 1408, "EVT_FOSSIL_FR_SL4_0" }, { 1409, "EVT_FOSSIL_FR_SL5_0" }, { 1410, "EVT_FOSSIL_FR_SM1_0" }, { 1411, "EVT_FOSSIL_FR_SR1_0" }, { 1412, "EVT_FOSSIL_FR_SR2_0" }, { 1413, "EVT_FOSSIL_FR_SR3_0" }, { 1414, "EVT_FOSSIL_FR_SR4_0" },
            { 1415, "EVT_FOSSIL_FR_SR5_0" }, { 1416, "EVT_FOSSIL_FR_SR6_0" }, { 1417, "EVT_FOSSIL_FR_SR7_0" }, { 1418, "EVT_FOSSIL_FR_DN1_0" }, { 1419, "EVT_FOSSIL_FR_DN2_0" }, { 1420, "EVT_FOSSIL_FR_DN3_0" }, { 1421, "EVT_FOSSIL_FR_DN4_0" }, { 1422, "EVT_FOSSIL_FR_ENT_0" }, { 1423, "EVT_FOSSIL_FR_TRP_0" }, { 1424, "EVT_FOSSIL_FR_SBW_0" },
            { 1425, "EVT_FOSSIL_FR_EXT_0" }, { 1450, "EVT_MAGE_BV_ENT_0" }, { 1451, "EVT_MAGE_BV_ABR_0" }, { 1452, "EVT_MAGE_BV_GVY_0" }, { 1453, "EVT_MAGE_BV_GVY_1" }, { 1454, "EVT_MAGE_BV_INN_0" }, { 1455, "EVT_MAGE_BV_ITS_0" }, { 1456, "EVT_MAGE_BV_WPN_0" }, { 1457, "EVT_MAGE_BV_ORT_0" }, { 1458, "EVT_MAGE_BV_CSK_0" },
            { 1459, "EVT_MAGE_BV_CSI_0" }, { 1460, "EVT_MAGE_BV_ITM_0" }, { 1461, "EVT_MAGE_BV_ZDN_0" }, { 1462, "EVT_MAGE_BV_EVE_0" }, { 1463, "EVT_MAGE_BV_FR1_0" }, { 1464, "EVT_MAGE_BV_FR2_0" }, { 1500, "EVT_PATA_T_CP_ENT_0" }, { 1501, "EVT_PATA_T_CP_GL1_0" }, { 1502, "EVT_PATA_T_CP_GL2_0" }, { 1503, "EVT_PATA_T_CP_STR_0" },
            { 1504, "EVT_PATA_T_CP_EXT_0" }, { 1505, "EVT_PATA_T_CP_SHN_0" }, { 1506, "EVT_PATA_T_CP_ALT_0" }, { 1507, "EVT_PATA_T_CP_STP_0" }, { 1508, "EVT_PATA_T_CP_ROM_0" }, { 1509, "EVT_PATA_T_CP_TOL_0" }, { 1550, "EVT_PATA_M_CM_MP0_0" }, { 1551, "EVT_PATA_M_CM_MP1_0" }, { 1552, "EVT_PATA_M_CM_MP2_0" }, { 1553, "EVT_PATA_M_CM_MP3_0" },
            { 1554, "EVT_PATA_M_CM_MP4_0" }, { 1555, "EVT_PATA_M_CM_MP5_0" }, { 1556, "EVT_PATA_M_CM_MP6_0" }, { 1557, "EVT_PATA_M_CM_MP7_0" }, { 1600, "EVT_SARI1_MS_ENT_0" }, { 1601, "EVT_SARI1_MS_FNT_0" }, { 1602, "EVT_SARI1_MS_EKH_0" }, { 1603, "EVT_SARI1_MS_PMR_0" }, { 1604, "EVT_SARI1_MS_MRR_0" }, { 1605, "EVT_SARI1_MS_MRR_2" },
            { 1606, "EVT_SARI1_MS_EIK_0" }, { 1607, "EVT_SARI1_MS_KTN_0" }, { 1608, "EVT_SARI1_MS_SCR_0" }, { 1609, "EVT_SARI1_MS_SSD_0" }, { 1610, "EVT_SARI1_MS_SSD_1" }, { 1650, "EVT_EVA1_IF_ENT_0" }, { 1651, "EVT_EVA1_IF_PUG_0" }, { 1652, "EVT_EVA1_IF_BAS_0" }, { 1653, "EVT_EVA1_IF_BAS_2" }, { 1654, "EVT_EVA1_IF_MKJ_0" },
            { 1655, "EVT_EVA1_IF_PTS_0" }, { 1656, "EVT_EVA1_IF_PTS_1" }, { 1657, "EVT_EVA1_IF_GGT_0" }, { 1658, "EVT_EVA1_IF_MSD_0" }, { 1659, "EVT_EVA1_IF_CST_0" }, { 1660, "EVT_EVA1_BF_FLS_0" }, { 1661, "EVT_EVA1_BF_DCK_0" }, { 1662, "EVT_EVA1_BF_DCK_1" }, { 1663, "EVT_EVA1_IF_MKJ_1" }, { 1750, "EVT_EVA2_IU_EVE_0" },
            { 1751, "EVT_EVA2_IU_EUG_0" }, { 1752, "EVT_EVA2_IU_EUG_1" }, { 1753, "EVT_EVA2_IU_EUG_2" }, { 1754, "EVT_EVA2_IU_ELV_0" }, { 1755, "EVT_EVA2_IU_SDV_0" }, { 1756, "EVT_EVA2_IU_SDV_1" }, { 1757, "EVT_EVA2_IF_ENT_1" }, { 1758, "EVT_EVA2_IF_PUG_1" }, { 1759, "EVT_EVA2_IF_BAS_1" }, { 1800, "EVT_ALEX3_AC_OHAKA" },
            { 1950, "EVT_KUWAN_QH_CAV_0" }, { 1951, "EVT_KUWAN_QH_HUH_0" }, { 1952, "EVT_KUWAN_QH_HUR_0" }, { 1953, "EVT_KUWAN_QH_LNI_0" }, { 808, "EVT_GATE_S_SG_ALX_5" }, { 853, "EVT_GATE_N_NG_TRA_1" }, { 854, "EVT_GATE_N_NG_BDA_1" }, { 855, "EVT_GATE_N_NG_BMA_1" }, { 856, "EVT_GATE_N_NG_MDA_1" }, { 1801, "EVT_ALEX3_AC_QUEEN" },
            { 1802, "EVT_ALEX3_AC_PRINCESS" }, { 1803, "EVT_ALEX3_AC_GUARD" }, { 1806, "EVT_ALEX3_AC_H2F" }, { 1807, "EVT_ALEX3_AC_ENT_2F" }, { 1808, "EVT_ALEX3_AC_LIB" }, { 1809, "EVT_ALEX3_AC_PARTY" }, { 1810, "EVT_ALEX3_AC_KITCHEN" }, { 1811, "EVT_ALEX3_AC_OUT_E" }, { 1812, "EVT_ALEX3_AC_OUT_C" }, { 1813, "EVT_ALEX3_AC_OUT_L" },
            { 1814, "EVT_ALEX3_AC_OUT_R" }, { 1815, "EVT_ALEX3_AC_OUT_C2" }, { 1816, "EVT_ALEX3_AC_OUT_C9" }, { 1817, "EVT_ALEX3_AC_PORT_A" }, { 1818, "EVT_ALEX3_AC_PORT_B" }, { 1819, "EVT_ALEX3_AC_PORT_C0" }, { 1820, "EVT_ALEX3_AC_TOWER_L1" }, { 1821, "EVT_ALEX3_AC_R_HALL" }, { 1822, "EVT_ALEX3_AC_LIB_2" }, { 1823, "EVT_ALEX3_AC_H2F_2" },
            { 1824, "EVT_ALEX3_AC_SWD_BTL" }, { 1850, "EVT_ALEX3_AT_STREET_A" }, { 1851, "EVT_ALEX3_AT_STREET_B" }, { 1852, "EVT_ALEX3_AT_STREET_C" }, { 1853, "EVT_ALEX3_AT_CENTER" }, { 1854, "EVT_ALEX3_AT_BACK_STR" }, { 1855, "EVT_ALEX3_AT_TSS" }, { 1856, "EVT_ALEX3_AT_GATE" }, { 1857, "EVT_ALEX3_AT_ITEM" }, { 1858, "EVT_ALEX3_AT_WEAPON" },
            { 1859, "EVT_ALEX3_AT_MAGIC" }, { 1860, "EVT_ALEX3_AT_INN" }, { 1861, "EVT_ALEX3_AT_PUB" }, { 1862, "EVT_ALEX3_AT_HOUSE_1" }, { 1863, "EVT_ALEX3_AT_HOUSE_2" }, { 1864, "EVT_ALEX3_AT_RUBY" }, { 1865, "EVT_ALEX3_AT_SENTOU" }, { 1866, "EVT_ALEX3_AT_WHARF" }, { 1900, "EVT_TRENO2_TR_BAR_0" }, { 1901, "EVT_TRENO2_TR_BHE_0" },
            { 1902, "EVT_TRENO2_TR_BHM_0" }, { 1903, "EVT_TRENO2_TR_BTA_0" }, { 1904, "EVT_TRENO2_TR_EKH_0" }, { 1905, "EVT_TRENO2_TR_FBH_0" }, { 1906, "EVT_TRENO2_TR_FKH_0" }, { 1907, "EVT_TRENO2_TR_FQH_0" }, { 1908, "EVT_TRENO2_TR_GAT_0" }, { 1909, "EVT_TRENO2_TR_KHA_0" }, { 1910, "EVT_TRENO2_TR_KHM_0" }, { 1911, "EVT_TRENO2_TR_QHM_0" },
            { 1912, "EVT_TRENO2_TR_RES_0" }, { 1913, "EVT_TRENO2_TR_TMS_0" }, { 1914, "EVT_TRENO2_TR_TOT_0" }, { 1915, "EVT_TRENO2_TR_WBH_0" }, { 1916, "EVT_TRENO2_TR_KHM_1" }, { 2000, "EVT_ALEX4_AC_HILDA2_A" }, { 2001, "EVT_ALEX4_AC_PRINCESS" }, { 2002, "EVT_ALEX4_AC_SAI_A" }, { 2003, "EVT_ALEX4_AC_SAI_B" }, { 2004, "EVT_ALEX4_AC_SAI_C" },
            { 2005, "EVT_ALEX4_AC_SAI_D" }, { 2006, "EVT_ALEX4_AC_SAI_E" }, { 2007, "EVT_ALEX4_AC_SAI_F" }, { 2008, "EVT_ALEX4_AC_SAI_G" }, { 2009, "EVT_ALEX4_AC_SEAT_R" }, { 2050, "EVT_ALEX4_AT_STREET_A" }, { 2051, "EVT_ALEX4_AT_STREET_B" }, { 2052, "EVT_ALEX4_AT_STREET_C" }, { 2053, "EVT_ALEX4_AT_CENTER" }, { 2054, "EVT_ALEX4_AT_GATE" },
            { 2055, "EVT_ALEX4_AT_IV_CTR_1" }, { 2100, "EVT_LIND3_TN_LB_GAT_0" }, { 2101, "EVT_LIND3_TN_LB_MTM_0" }, { 2102, "EVT_LIND3_TN_LB_MST_0" }, { 2103, "EVT_LIND3_TN_LB_IN1_0" }, { 2104, "EVT_LIND3_TN_LB_IN2_0" }, { 2105, "EVT_LIND3_TN_LB_BAZ_0" }, { 2106, "EVT_LIND3_TN_LB_HUS_0" }, { 2107, "EVT_LIND3_TN_LB_PLZ_0" },
            { 2108, "EVT_LIND3_TN_LB_ECH_0" }, { 2109, "EVT_LIND3_TN_LB_WPN_0" }, { 2110, "EVT_LIND3_TN_LB_STN_0" }, { 2111, "EVT_LIND3_TN_LB_ATL_0" }, { 2112, "EVT_LIND3_TN_LB_TTL_0" }, { 2113, "EVT_LIND3_TN_LB_RFS_0" }, { 2114, "EVT_LIND3_TN_LB_HIG_0" }, { 2150, "EVT_LIND3_CS_LB_CFR_0" }, { 2151, "EVT_LIND3_CS_LB_LPF_0" },
            { 2152, "EVT_LIND3_CS_LB_EXB_0" }, { 2153, "EVT_LIND3_CS_LB_LLP_0" }, { 2155, "EVT_LIND3_CS_LB_PLA_0" }, { 2157, "EVT_LIND3_CS_LB_HNG_0" }, { 2158, "EVT_LIND3_CS_LB_SHB_0" }, { 2159, "EVT_LIND3_CS_LB_BRG_0" }, { 2160, "EVT_LIND3_CS_LB_CTM_0" }, { 2161, "EVT_LIND3_CS_LB_GRM_0" }, { 2162, "EVT_LIND3_CS_LB_MHW_0" },
            { 2163, "EVT_LIND3_CS_LB_HAL_0" }, { 2164, "EVT_LIND3_CS_LB_ASD_1" }, { 2167, "EVT_LIND3_CS_LB_LPR_0" }, { 2168, "EVT_LIND3_CS_LB_UHW_0" }, { 2169, "EVT_LIND3_CS_LB_MET_0" }, { 2170, "EVT_LIND3_CS_LB_HFC_0" }, { 2171, "EVT_LIND3_CS_LB_LFT_0" }, { 2172, "EVT_LIND3_CS_LB_OBS_M" }, { 2173, "EVT_LIND3_CS_LB_POT_M" },
            { 2200, "EVT_SUNA_SP_RRO_0" }, { 2201, "EVT_SUNA_SP_RPW_0" }, { 2202, "EVT_SUNA_SP_RPE_0" }, { 2203, "EVT_SUNA_SP_RRM_0" }, { 2204, "EVT_SUNA_SP_RFG_0" }, { 2205, "EVT_SUNA_SP_RSW_0" }, { 2206, "EVT_SUNA_SP_REX_0" }, { 2207, "EVT_SUNA_SP_KPW_0" }, { 2208, "EVT_SUNA_SP_KDR_0" }, { 2209, "EVT_SUNA_SP_KKR_0" },
            { 2211, "EVT_SUNA_SP_KDC_0" }, { 2212, "EVT_SUNA_SP_KEX_0" }, { 2213, "EVT_SUNA_SP_MP1_0" }, { 2214, "EVT_SUNA_SP_MP2_0" }, { 2215, "EVT_SUNA_SP_MT1_0" }, { 2216, "EVT_SUNA_SP_MP3_0" }, { 2217, "EVT_SUNA_SP_MHL_0" }, { 2220, "EVT_SUNA_SP_MT2_0" }, { 2221, "EVT_SUNA_SP_MT3_0" }, { 2222, "EVT_SUNA_SP_MBS_0" },
            { 2250, "EVT_OEIL_UV_EXT_0" }, { 2251, "EVT_OEIL_UV_ENT_0" }, { 2252, "EVT_OEIL_UV_PST_0" }, { 2253, "EVT_OEIL_UV_DP1_0" }, { 2254, "EVT_OEIL_UV_DP2_0" }, { 2255, "EVT_OEIL_UV_ELV_0" }, { 2256, "EVT_OEIL_UV_VSL_0" }, { 2257, "EVT_OEIL_UV_VSL_1" }, { 2258, "EVT_OEIL_UV_RFM_0" }, { 2259, "EVT_OEIL_UV_DEP_0" },
            { 2260, "EVT_OEIL_UV_FDP_0" }, { 2261, "EVT_OEIL_G1_BRG_0" }, { 2300, "EVT_EST_EG_EXT_0" }, { 2301, "EVT_EST_EG_RM1_0" }, { 2302, "EVT_EST_EG_RM1_1" }, { 2303, "EVT_EST_EG_RM2_0" }, { 2304, "EVT_EST_EG_OBS_0" }, { 2305, "EVT_EST_EG_EGV_0" }, { 2350, "EVT_GULUGU_GV_WHL_0" }, { 2351, "EVT_GULUGU_GV_IDO_0" },
            { 2352, "EVT_GULUGU_GV_RM1_0" }, { 2353, "EVT_GULUGU_GV_RM2_0" }, { 2354, "EVT_GULUGU_GV_RM3_0" }, { 2355, "EVT_GULUGU_GV_RM4_0" }, { 2356, "EVT_GULUGU_GV_ATK_0" }, { 2357, "EVT_GULUGU_GV_HLD_0" }, { 2358, "EVT_GULUGU_GV_PW1_0" }, { 2359, "EVT_GULUGU_GV_PW2_0" }, { 2360, "EVT_GULUGU_GV_PW3_0" }, { 2361, "EVT_GULUGU_GV_TIN_0" },
            { 2362, "EVT_GULUGU_GV_MGS_0" }, { 2363, "EVT_GULUGU_GV_PW4_0" }, { 2364, "EVT_GULUGU_GV_MG1_0" }, { 2365, "EVT_GULUGU_GV_MG2_0" }, { 2400, "EVT_ALEX5_AC_PORT_A" }, { 2401, "EVT_ALEX5_AC_PORT_B" }, { 2402, "EVT_ALEX5_AC_PORT_C0" }, { 2403, "EVT_ALEX5_AC_PORT_C1" }, { 2404, "EVT_ALEX5_AC_OUT_C" }, { 2405, "EVT_ALEX5_AC_OUT_L" },
            { 2406, "EVT_ALEX5_AC_TOWER_L1" }, { 2450, "EVT_ALEX5_AT_STREET_A" }, { 2451, "EVT_ALEX5_AT_STREET_C" }, { 2452, "EVT_ALEX5_AT_CENTER" }, { 2453, "EVT_ALEX5_AT_BACK_STR" }, { 2454, "EVT_ALEX5_AT_TSS" }, { 2455, "EVT_ALEX5_AT_INN" }, { 2456, "EVT_ALEX5_AT_SENTOU" }, { 2457, "EVT_ALEX5_AT_RUBY" }, { 2458, "EVT_ALEX5_AT_WHARF" },
            { 2500, "EVT_IPSEN_IP_EXT_0" }, { 2502, "EVT_IPSEN_IP_EN1_0" }, { 2503, "EVT_IPSEN_IP_EN2_0" }, { 2504, "EVT_IPSEN_IP_RM1_0" }, { 2505, "EVT_IPSEN_IP_RM2_0" }, { 2506, "EVT_IPSEN_IP_HL1_0" }, { 2507, "EVT_IPSEN_IP_HL2_0" }, { 2508, "EVT_IPSEN_IP_ST1_0" }, { 2509, "EVT_IPSEN_IP_ST2_0" }, { 2510, "EVT_IPSEN_IP_CNT_0" },
            { 2512, "EVT_IPSEN_IP_CNT_2" }, { 2513, "EVT_IPSEN_IP_SMN_0" }, { 2550, "EVT_SHRINE_ES_ENT_0" }, { 2551, "EVT_SHRINE_ES_ENT_1" }, { 2552, "EVT_SHRINE_ES_SEL_1" }, { 2553, "EVT_SHRINE_ES_SEL_2" }, { 2554, "EVT_SHRINE_ES_TRP_0" }, { 2600, "EVT_TERA_TA_THL_0" }, { 2601, "EVT_TERA_TA_HLD_0" }, { 2602, "EVT_TERA_TA_BST_0" },
            { 2603, "EVT_TERA_TA_PRB_0" }, { 2604, "EVT_TERA_TA_RPB_0" }, { 2605, "EVT_TERA_TA_ROU_0" }, { 2606, "EVT_TERA_TA_ROD_0" }, { 2607, "EVT_TERA_TA_TOB_0" }, { 2608, "EVT_TERA_TA_IVK_0" }, { 2650, "EVT_BAL_BB_STW_0" }, { 2651, "EVT_BAL_BB_SQR_0" }, { 2652, "EVT_BAL_BB_FGT_0" }, { 2653, "EVT_BAL_BB_CDR_0" },
            { 2654, "EVT_BAL_BB_CDL_0" }, { 2655, "EVT_BAL_BB_WPS_0" }, { 2656, "EVT_BAL_BB_GTH_0" }, { 2657, "EVT_BAL_BB_ITS_0" }, { 2658, "EVT_BAL_BB_UDL_0" }, { 2659, "EVT_BAL_BB_BRG_0" }, { 2660, "EVT_BAL_BB_THL_0" }, { 2661, "EVT_BAL_BB_INV_0" }, { 2700, "EVT_PANDEMO_PD_STR_0" }, { 2701, "EVT_PANDEMO_PD_CAO_0" },
            { 2702, "EVT_PANDEMO_PD_EAO_0" }, { 2703, "EVT_PANDEMO_PD_SAO_0" }, { 2704, "EVT_PANDEMO_PD_AOS_0" }, { 2705, "EVT_PANDEMO_PD_BWR_0" }, { 2706, "EVT_PANDEMO_PD_RM1_0" }, { 2707, "EVT_PANDEMO_PD_MCD_0" }, { 2708, "EVT_PANDEMO_PD_RM2_0" }, { 2709, "EVT_PANDEMO_PD_OLB_0" }, { 2710, "EVT_PANDEMO_PD_ULB_0" },
            { 2711, "EVT_PANDEMO_PD_OEV_0" }, { 2712, "EVT_PANDEMO_PD_ELV_0" }, { 2713, "EVT_PANDEMO_PD_EVD_0" }, { 2714, "EVT_PANDEMO_PD_MZM_0" }, { 2715, "EVT_PANDEMO_PD_RTA_0" }, { 2716, "EVT_PANDEMO_PD_RTB_0" }, { 2717, "EVT_PANDEMO_PD_RTC_0" }, { 2718, "EVT_PANDEMO_PD_ALB_0" }, { 2719, "EVT_PANDEMO_PD_PEV_0" },
            { 2720, "EVT_PANDEMO_PD_RTB_1" }, { 2750, "EVT_INV_IV_BRG_0" }, { 2753, "EVT_INV_IV_CXI_0" }, { 2800, "EVT_DAG_DG_ENT_0" }, { 2801, "EVT_DAG_DG_SRH_0" }, { 2802, "EVT_DAG_DG_LFT_0" }, { 2803, "EVT_DAG_DG_2F0_0" }, { 2850, "EVT_HILDA3_G3_BRG_0" }, { 2851, "EVT_HILDA3_G3_EGR_0" }, { 2852, "EVT_HILDA3_G3_DCK_0" },
            { 2853, "EVT_HILDA3_G3_EXT_0" }, { 2854, "EVT_HILDA3_G3_BRG_1" }, { 2855, "EVT_HILDA3_BN_DCR_0" }, { 2856, "EVT_HILDA3_BN_DCR_1" }, { 2951, "EVT_CHOCO_CH_HLG_0" }, { 2952, "EVT_CHOCO_CH_FGD_0" }, { 2954, "EVT_CHOCO_CH_PDS_0" }, { 2955, "EVT_CHOCO_CH_PDS_1" }, { 3100, "EVT_MOGNET_MOG_VER1" }, { 1700, "EVT_SARI2_MS_ENT_1" },
            { 1701, "EVT_SARI2_MS_FNT_1" }, { 1702, "EVT_SARI2_MS_EKH_1" }, { 1703, "EVT_SARI2_MS_PMR_1" }, { 1704, "EVT_SARI2_MS_MRR_1" }, { 1705, "EVT_SARI2_MS_EIK_1" }, { 1706, "EVT_SARI2_MS_KTN_1" }, { 1707, "EVT_SARI2_MS_SCR_1" }, { 1917, "EVT_TRENO2_TR_KHM_2" }, { 2154, "EVT_LIND3_CS_LB_POT_0" }, { 2165, "EVT_LIND3_CS_LB_ASD_2" },
            { 2166, "EVT_LIND3_CS_LB_OBS_0" }, { 2501, "EVT_IPSEN_IP_EXT_1" }, { 2751, "EVT_INV_IV_BRG_1" }, { 2752, "EVT_INV_IV_BRG_2" }, { 2754, "EVT_INV_IV_CTR_0" }, { 2755, "EVT_INV_G3_BRG_2" }, { 2756, "EVT_INV_RR_BRG_1" }, { 2900, "EVT_LAST_CW_ENT_0" }, { 2901, "EVT_LAST_CW_ETH_0" }, { 2902, "EVT_LAST_CW_1LY_0" },
            { 2903, "EVT_LAST_CW_1LY_1" }, { 2904, "EVT_LAST_CW_1LY_2" }, { 2905, "EVT_LAST_CW_GIA_0" }, { 2906, "EVT_LAST_CW_2LY_0" }, { 2907, "EVT_LAST_CW_2LY_1" }, { 2908, "EVT_LAST_CW_GIA_1" }, { 2909, "EVT_LAST_CW_3LY_0" }, { 2910, "EVT_LAST_CW_3LY_1" }, { 2911, "EVT_LAST_CW_3LY_2" }, { 2912, "EVT_LAST_CW_GIA_2" },
            { 2913, "EVT_LAST_CW_4LY_0" }, { 2914, "EVT_LAST_CW_4LY_1" }, { 2915, "EVT_LAST_CW_GIA_3" }, { 2916, "EVT_LAST_CW_5LY_0" }, { 2917, "EVT_LAST_CW_GIA_4" }, { 2918, "EVT_LAST_CW_LTP_0" }, { 2919, "EVT_LAST_CW_EMP_0" }, { 2920, "EVT_LAST_CW_SPC_0" }, { 2921, "EVT_LAST_CW_SPC_1" }, { 2922, "EVT_LAST_CW_CYW_0" },
            { 2923, "EVT_LAST_CW_CYW_1" }, { 2924, "EVT_LAST_CW_CYW_2" }, { 2925, "EVT_LAST_CW_CYW_3" }, { 2926, "EVT_LAST_CW_LST_0" }, { 2927, "EVT_LAST_CW_LST_A" }, { 2928, "EVT_LAST_CW_LST_1" }, { 2929, "EVT_LAST_CW_MBG_A" }, { 2930, "EVT_LAST_CW_MBG_0" }, { 2931, "EVT_LAST_G3_BRG_0" }, { 2932, "EVT_LAST_RR_BRG_0" },
            { 2933, "EVT_LAST_CW_MBG_1" }, { 2934, "EVT_LAST_CW_MBG_2" }, { 3000, "EVT_ENDING_AT_1" }, { 3001, "EVT_ENDING_AC_1" }, { 3002, "EVT_ENDING_AC_2" }, { 3003, "EVT_ENDING_AC_3" }, { 3004, "EVT_ENDING_BU_1" }, { 3005, "EVT_ENDING_KM_1" }, { 3006, "EVT_ENDING_LB_1" }, { 3007, "EVT_ENDING_TR_1" }, { 3008, "EVT_ENDING_TH_0" },
            { 3009, "EVT_ENDING_TH_1" }, { 3010, "EVT_ENDING_TH_2" }, { 3011, "EVT_ENDING_TH_3" }, { 3012, "EVT_ENDING_AC_4" }, { 3050, "EVT_MAGE2_BV_ENT_0" }, { 3051, "EVT_MAGE2_BV_ABR_0" }, { 3052, "EVT_MAGE2_BV_GVY_0" }, { 3053, "EVT_MAGE2_BV_INN_0" }, { 3054, "EVT_MAGE2_BV_ITS_0" }, { 3055, "EVT_MAGE2_BV_WPN_0" },
            { 3056, "EVT_MAGE2_BV_ORT_0" }, { 3057, "EVT_MAGE2_BV_CSK_0" }, { 3058, "EVT_MAGE2_BV_CSI_0" }, { 3059, "EVT_MAGE2_BV_ITM_0" }
        };
            List<FieldIndex> FieldIndices = new List<FieldIndex>(); //  temp obj
            List<Datapoint> Dataset = new List<Datapoint>();        //  perm obj
            List<FieldIndex> FinalFieldIndices = new List<FieldIndex>();    //  perm obj
            List<Datapoint> DS3 = new List<Datapoint>();      // perm
            List<FieldData> MainFieldIndex = new List<FieldData>(); //perm
            List<Datapoint> DataSIMP = new List<Datapoint>();
            //ListFI FieldIndicesObj = new ListFI(FieldIndices);
            //ListFI FinalFieldIndicesObj = new ListFI(FieldIndices);
            int[] deadzone = { 33122111, 34426256 };    //  last file byte 68034959 0x40e218f
            byte[] pattern1a_preload = new byte[] { 253, 0, 5 };        //var zone 1a x
            byte[] pattern1b_setvar = new byte[] { 5, 217, 21, 125 };   //var zone 1b x
            byte[] pattern2a_setent = new byte[] { 5, 216, 2, 125 };    //var zone 2a y
            byte[] pattern2a2_d802 = new byte[] { 5, 216, 2, 216, 2 };    //D802 x2 for double entrance
            byte[] pattern2a3_7e = new byte[] { 5, 216, 2, 126 };       //L long bytes (still only two bytes)
            byte[] pattern2b_field = new byte[] { 44, 127, 43, 0 };     //var zone 2b x
            byte[] pattern3_return = new byte[] { 4, 255 };             // returns are just 4, not 4, 0
            byte[] pattern4_loop = new byte[] { 1, 255, 255, 0, 0 };    // ignore 255s
            byte[] pattern5_range = new byte[] { 4, 5, 122, 2, 127, 3, 1, 0, 4 };  // range*_region
            byte[] pattern6_init = new byte[] { 1, 2, 0, 0, 8, 0, 2, 0, 255, 255, 41, 0, 255 };  // range*_init  offset between init and range for init data.
            //  read 255s first two is Uint16 of initdata after this pattern ends, last one is number of Uint16 pairs, so times by 4 to get length of data
            byte[] pattern6_BTNinit = new byte[] { 1, 3, 0, 0, 12, 0, 2, 0, 255, 255, 3, 0, 255 };  // range*_init  offset between init and range for init data.
            //                                      +1      +4                      +10
            byte[] pattern6_404init = new byte[] { 1, 3, 0, 0, 12, 0, 2, 0, 255, 255, 22, 0, 255 };  // range*_init  offset between init and range for init data.
            byte[] pattern7_Tri127 = new byte[] { 39, 0, 127 };  // SetTriangleFlagMask
            byte[] pattern7_OpCea = new byte[] { 234 };  // EA OpCode
            byte[] pattern7_OpCa9 = new byte[] { 169, 0, 250 };  // A9 OpCode
            int[] pattern8_noPreload = new int[] { 552, 555, 1302, 1305, 2922, 2923, 2924, 2925 };    //  thse need special allowance through all pattern1 loops, and them dummy data in place of preload patterns
            int[] windowAsyncFP = new int[] { 44, 127, 32, 0, 1, 16, 38 }; // 2C 7F 20 00 01 10 26 WindowAsync False Positive in char loops
            /*
            int[] progression = new int[] 
            { 
                50, 104, 105, 106, 115, 150, 55, 54, 51, 58, 69, 209, 206, 251, 207, 208, 201, 204, 203, 202, 200, 252, 257, 261, 262, 300, 301, 307, 309, 312, 352, 354, 355, 358, 353, 356, 404, 406, 403, 400, 401, 456, 455,
                457, 452, 451, 454, 504, 507, 500, 506, 503, 614, 600, 571, 607, 554, 565, 1365, 611, 613, 612, 615, 575, 563, 550, 551, 552, 555, 556, 559, 566, 567, 568, 569, 570, 572, 618, 701, 703, 705, 707, 800, 801, 750, 752, 755, 757, 753, 758,
                760, 768, 812, 810, 809, 807, 908, 904, 909, 900, 916, 913, 914, 950, 951, 953, 952, 1201, 1000, 1001, 1051, 1059, 1055, 1052, 1060, 1107, 1101, 1010, 1110, 1153, 1150, 1208, 1211, 1206, 1205, 1225, 1203, 1204, 955, 1250, 1251, 1254,
                1255, 1300, 1307, 1315, 1352, 1422, 1425, 3100, 1501, 1505, 1463, 1451, 1454, 1450, 1500, 1503, 1504, 1550, 1554, 1555, 1600, 1601, 1607, 1602, 1605, 1606, 1650, 1652, 1754, 1755, 1756, 1757, 1608, 1610, 1658, 1657, 1654, 1661, 1663,
                1662, 1659, 1861, 1854, 1864, 1819, 1803, 1807, 1866, 1816, 1905, 1903, 2054, 2050, 2007, 2000, 2006, 2055, 2105, 2161, 2150, 2172, 2169, 2211, 2107, 2111, 2114, 2855, 2856, 1452, 1458, 2200, 2202, 2201, 2209, 2261, 2250, 2251, 2253,
                2254, 2257, 2258, 2259, 2260, 2204, 2205, 2213, 2222, 2207, 2212, 2301, 2357, 2157, 1800, 2800, 2850, 2854, 2500, 2502, 2512, 2510, 2504, 2505, 2551, 2851, 2852, 2550, 2853, 2601, 2603, 2604, 2605, 2606, 2607, 2651, 2653, 2654, 2657,
                2656, 2658, 2652, 2700, 2701, 2706, 2710, 2711, 2717, 2719, 2660, 2750, 2751, 3050, 2753, 2756, 2900, 2904, 2905, 2907, 2908, 2912, 2914, 2915, 2917, 2918, 2919, 2920, 2921, 2922, 2926, 2934 
            };   //  this is the required fields to advance the story. some are used multiple times, as long as we have a complete path once, the user can walk backwards when needed.
            List<int[]> CityArr = new List<int[]>()
            {
                new int[] { 50, 63 },new int[] { 64, 0 },new int[] { 65, 67 },new int[] { 64, 0 },new int[] { 68, 69 },new int[] { 70, 0 },new int[] { 100, 117 },new int[] { 150, 167 },new int[] { 200, 209 },new int[] { 250, 262 },new int[] { 300, 312 },
                new int[] { 350, 454 },new int[] { 455, 457 },new int[] { 500, 507 },new int[] { 550, 576 },new int[] { 600, 620 },new int[] { 650, 661 },new int[] { 701, 707 },new int[] { 750, 768 },new int[] { 800, 850 },new int[] { 851, 852 },
                new int[] { 1256, 0 },new int[] { 2950, 0 },new int[] { 2953, 0 },new int[] { 152, 0 },new int[] { 656, 663 },new int[] { 769, 0 },new int[] { 803, 816 },new int[] { 900, 932 },new int[] { 950, 957 },new int[] { 1000, 1110 },new int[] { 1150, 1153 },
                new int[] { 1200, 1250 },new int[] { 1251, 1255 },new int[] { 1300, 1315 },new int[] { 1350, 1370 },new int[] { 1400, 1425 },new int[] { 1450, 1462 },new int[] { 1463, 1464 },new int[] { 1500, 1509 },new int[] { 1500, 1557 },new int[] { 1600, 1610 },
                new int[] { 1650, 1659 },new int[] { 1660, 1662 },new int[] { 1663, 1759 },new int[] { 1800, 0 },new int[] { 1950, 1953 },new int[] { 808, 854 },new int[] { 855, 856 },new int[] { 1801, 1824 },new int[] { 1850, 1866 },new int[] { 1900, 1916 },
                new int[] { 2000, 0 },new int[] { 2001, 2009 },new int[] { 2050, 2054 },new int[] { 2055, 0 },new int[] { 2100, 2114 },new int[] { 2150, 2173 },new int[] { 2200, 2222 },new int[] { 2250, 2261 },new int[] { 2300, 2305 },new int[] { 2350, 2365 },
                new int[] { 2400, 2406 },new int[] { 2450, 2458 },new int[] { 2500, 2513 },new int[] { 2550, 2554 },new int[] { 2600, 2608 },new int[] { 2650, 2661 },new int[] { 2700, 2720 },new int[] { 2750, 2753 },new int[] { 2800, 2803 },new int[] { 2850, 2854 },
                new int[] { 2855, 2856 },new int[] { 2951, 0 },new int[] { 2952, 0 },new int[] { 2954, 2955 },new int[] { 3100, 0 },new int[] { 1700, 1707 },new int[] { 1917, 0 },new int[] { 2154, 2166 },new int[] { 2501, 0 },new int[] { 2751, 2754 },new int[] { 2755, 0 },
                new int[] { 2756, 0 },new int[] { 2900, 2921 },new int[] { 2922, 2927 },new int[] { 2928, 0 },new int[] { 2929, 2934 },new int[] { 3000, 3012 },new int[] { 3050, 3059 }
            }; //71 not including single fields, 
            int[] CitySize = new int[]
            {
                14, 1, 3, 2, 1, 18, 17, 10, 13, 13, 24, 3, 8, 27, 21, 9, 7, 19, 7, 2, 1, 1, 1, 1, 5, 1, 10, 20, 8, 41, 4, 29, 5, 16, 21, 26, 13, 2, 10, 8, 11, 
                10, 3, 11, 1, 4, 3, 2, 22, 17, 17, 1, 9, 5, 1, 15, 20, 20, 12, 6, 16, 7, 9, 12, 5, 9, 12, 21, 2, 4, 5, 2, 1, 1, 2, 1, 8, 1, 3, 1, 3, 1, 1, 22, 6, 1, 6, 13, 10
            };  //  average of 9.18, 11.33 not including single field cities, 12.36 not including 2 rooms, 13.35 not including 3
            */
            int[] FWMarr = new int[] 
            { 
                262, 300, 311, 312, 350, 450, 455, 503, 550, 602, 650, 701, 706, 707, 750, 800, 806, 850, 851, 
                852, 1256, 2950, 2953, 769, 807, 908, 1000, 1300, 1352, 1425, 1450, 1463, 1500, 1555, 1557, 1600, 
                1650, 1757, 1950, 1953, 853, 854, 855, 856, 1856, 1908, 2100, 2152, 2164, 2173, 2211, 2212, 2250, 
                2261, 2300, 2403, 2450, 2500, 2550, 2750, 2753, 2800, 2850, 2851, 2852, 2853, 2854, 2855, 2856, 
                2951, 2952, 2954, 3100, 1700, 2165, 2501, 2751, 2901, 3050 
            };  //  these fields have an exit to a world map // 79 
            List<int> FWMl = new List<int>();
            foreach (int fwmi in FWMarr) { FWMl.Add(fwmi); }

            List<int[]> WorldConnectors = new List<int[]>()
            {
                new int[] { 262,300,852 },new int[] { 311,350,455,806 },new int[] { 550, 1256 },new int[] { 602,650,701,850,2950 },new int[] { 706 },
                new int[] { 707,750,851 },new int[] { 311,350,455,806 },new int[] { 807,908,1950 },new int[] { 707,750,851,1000 },new int[] { 550, 1256 },
                new int[] { 602,650,701,850,2950 },new int[] { 650,1425,1463,1500 },new int[] { 650,1450,1463,1500 },new int[] { 1557, 1600 },new int[] { 1557, 1600 },
                new int[] { 1555, 1650 },new int[] { 1555, 1650 },new int[] { 1557, 1600 },new int[] { 1557, 1600 },new int[] { 1555, 1757 },new int[] { 1856 },
                new int[] { 807,1908,1950 },new int[] { 650,701,707,750,854,855,1450,1500,1600,2152,2200,2300,2403,2950 },new int[] { 2250 },
                new int[] { 650, 701, 707, 750, 854, 855, 1450, 1500, 1600, 2152, 2300, 2403, 2950 },
                new int[] { 650, 701, 706, 707, 750, 807, 854, 855, 1450, 1500, 1555, 1600, 1757, 1856, 1908, 1950, 2152, 2250, 2300, 2403, 2500, 2800, 2950 },
                new int[] { 650, 701, 706, 707, 750, 807, 854, 855, 1450, 1500, 1555, 1600, 1757, 1856, 1908, 1950, 2152, 2250, 2300, 2403, 2500, 2800, 2950, 2550, 2551, 2553, 2554 },
                new int[] { 650, 701, 706, 707, 750, 807, 854, 855, 1450, 1500, 1555, 1600, 1757, 1856, 1908, 1950, 2152, 2250, 2300, 2403, 2500, 2800, 2950 },
                new int[] { 650, 701, 706, 707, 750, 807, 854, 855, 1450, 1500, 1555, 1600, 1856, 1908, 1950, 2152, 2250, 2300, 2403, 2500, 2800, 2950 },
                new int[] { 650, 701, 706, 707, 750, 807, 854, 855, 1450, 1500, 1555, 1600, 1856, 1908, 1950, 2152, 2250, 2300, 2403, 2500, 2800, 2901, 2950 }
            };  //  these fields connect to each other from the world map side, they change as the story progresses


            int[] blank2ints = new int[] { 0, 0 };
            byte[] blank2bytes = new byte[] { 0, 0 };
            int[][] raddresses = { blank2ints, blank2ints, blank2ints, blank2ints, blank2ints, blank2ints, blank2ints, };
            int[] pat1a, pat1b, pat2a, pat2b = blank2ints;
            byte[] pat1abytes, pat1bbytes, pat2abytes, pat2bbytes = blank2bytes;
            
            byte[] ba_p0data7 = rand9er.ba_p0data7;
            //Console.WriteLine(ba_p0data7.Length);
            //Console.WriteLine(progression.Length); //281 34%

            //      Funcs       //
            (int key, string value, byte[] asc2) DictBytesConvert(int i)
            {
                KeyValuePair<int, string> fieldSet = ff9Fields.ElementAt(i);
                byte[] ascii = Encoding.ASCII.GetBytes(fieldSet.Value);
                byte[] ascii2 = new byte[] { 46, 101, 98 }; //   add "2e 65 62" ".eb"
                byte[] ascii3 = new byte[ascii.Length + ascii2.Length];
                ascii.CopyTo(ascii3, 0);
                ascii2.CopyTo(ascii3, ascii.Length);
                return (key: fieldSet.Key, value: fieldSet.Value, asc2: ascii3);
            }
            void CheckSumChecks(string opt)
            {
                //if (j == 1) for (int i = 0; i < FieldIndices.Count; i++) FieldIndices[i].SumCheck();
                if (opt == "ffi") for (int i = 0; i < FinalFieldIndices.Count; i++) { FinalFieldIndices[i].SumCheck(); }
                if (opt == "ds") for (int i = 0; i < Dataset.Count; i++) { Dataset[i].SumCheck(); }
            }
            //       Cache      //
            void CacheCheck()
            {
                string md5cacheFI = Md5();

                //FIELD INDEX CACHE
                if (md5cacheFI == "FFIO match")
                {
                Console.WriteLine("Using FieldIndex Cache");
                    CacheIO("read SB_FFIO");
                }
                if (md5cacheFI == "FFIO nomatch")     //  manually genearte new FI cache for unkown p0data7
                {
                    Console.WriteLine("Generating New Cache Please wait");
                    var t = Task.Run(async delegate { await Task.Delay(600); }); t.Wait();
                    GenFieldIndex();    //  ONLY TIME TO RUN THIS
                    GenFinalFieldIndex();   //  ONLY TIME TO RUN THIS
                }
                //      OTHER CACHE
               //i think check if user cache IS different and we generate it, perhaps make an inclusion for a usercache check
                //currently if no md5 match, manual recreation everytime. need a way of storing the cache hash to skip recreation for user caches

            }
            void CacheIO(string opt)
            {
                string ffio = "\\SB_FFIO_reg.bin";
                if (rand9er.c_mogBool)
                {
                    ffio = "\\SB_FFIO_mog.bin";
                }
                string sffio = rand9er.tb_flText + ffio;

                if (opt == "write SB_FFIO") //  serialize
                {
                    //FinalFieldIndices = new List<FieldIndex>(FinalFieldIndices);
                    FileStream fs = new FileStream(sffio, FileMode.Create);
                    BinaryFormatter formatter = new BinaryFormatter();
                    try { formatter.Serialize(fs, FinalFieldIndices); }
                    catch (System.Runtime.Serialization.SerializationException e) { Console.WriteLine("SerialF: " + e.Message); throw; }
                    catch (System.Security.SecurityException e) { Console.WriteLine("SerialF: " + e.Message); throw; }
                    catch (ArgumentNullException e) { Console.WriteLine("SerialF: " + e.Message); throw; }
                    finally { fs.Close(); }
                }
                if (opt == "read SB_FFIO")  //  deserialize
                {
                    FileStream fs = new FileStream(sffio, FileMode.Open);
                    BinaryFormatter formatter = new BinaryFormatter();
                    try { FinalFieldIndices = (List<FieldIndex>)formatter.Deserialize(fs); }
                    catch (System.Runtime.Serialization.SerializationException e) { Console.WriteLine("SerialF: " + e.Message); throw; }
                    catch (System.Security.SecurityException e) { Console.WriteLine("SerialF: " + e.Message); throw; }
                    catch (ArgumentNullException e) { Console.WriteLine("SerialF: " + e.Message); throw; }
                    finally { fs.Close(); }
                    //FinalFieldIndices = FieldIndicesObj.Filist;

                }
                if (opt == "restore SB_FFIO")
                {
                    byte[] sb_ffio = Resources.Caches.SB_FFIO_reg;
                    if (rand9er.c_mogBool)
                    {
                        sb_ffio = Resources.Caches.SB_FFIO_mog;
                    }
                    File.WriteAllBytes(sffio, sb_ffio);
                }

                //  end of CacheIO()
            }
            string Md5()
            {
                //  CURRENT
                // p0data7.bin    mog  99a8a52d4abdfcedc570b948c1ff8a75     fresh mog install
                // p0data7.bin    reg  df4d0b9acd66df48452c40028e29b8ef     fresh install
                // SB_FFIO_mog.bin     8238E89E368D90C6E718EE746A27EDA8     non ListFI, just List<FieldIndex>
                // SB_FFIO_reg.bin     afe51d2780ec3687fc833638bd27f157     non ListFI, just List<FieldIndex>

                //  UNEEDED
                // SB_FIO_reg.bin      e62b65f7bc9e8221b030fcc564b3512c     fresh
                // SB_FIO_mog.bin mog  f1c48c76ef1ae9f383979108b0c1489c    checked
                // SB_FFIO_reg.bin     53202caa6cc6dea598fd37364815c752     verified    ListFI
                // SB_FFIO_mog.bin     8238e89e368d90c6e718ee746a27eda8     updated     ListFI

                string md5cacheFI = "FFIO nomatch";
                string md5p7 = "df4d0b9acd66df48452c40028e29b8ef";
                string dirp7 = "\\StreamingAssets\\p0data7.bin";
                string md5p7w = "";
                string md5ficache = "afe51d2780ec3687fc833638bd27f157";
                string dirCache = "\\SB_FFIO_reg.bin";
                string md5Cache = "";
                if (rand9er.c_mogBool)
                {
                    md5p7 = "99a8a52d4abdfcedc570b948c1ff8a75";
                    dirp7 = "\\MoguriFiles\\StreamingAssets\\p0data7.bin";
                    md5ficache = "8238e89e368d90c6e718ee746a27eda8";
                    dirCache = "\\SB_FFIO_mog.bin";
                }

                //  md5 action
                using (var md5 = MD5.Create())
                {

                    //  md5 p0data7.bin
                    using (var stream = File.OpenRead(rand9er.tb_flText + dirp7))
                    { md5p7w = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant(); }
                    Console.WriteLine("p0data7.bin:" + md5p7w + " rang9er.c_mogBool: " + rand9er.c_mogBool);

                    //  if cache file exists, md5 it also
                    if (File.Exists(dirCache))
                    {
                        using (var stream2 = File.OpenRead(rand9er.tb_flText + dirCache))
                        { md5Cache = BitConverter.ToString(md5.ComputeHash(stream2)).Replace("-", "").ToLowerInvariant(); }
                        Console.WriteLine("cache md5:" + md5Cache + " rang9er.c_mogBool: " + rand9er.c_mogBool);
                    }
                    else if (!File.Exists(dirCache))
                    {
                        if (md5p7 == md5p7w)
                        {
                            //restore cache files from inside program for known p0data7 md5
                            CacheIO("restore SB_FFIO");
                            Console.WriteLine("Restored Cache from inside program");
                            // md5 newly created cache
                            using (var stream2 = File.OpenRead(rand9er.tb_flText + dirCache))
                            { md5Cache = BitConverter.ToString(md5.ComputeHash(stream2)).Replace("-", "").ToLowerInvariant();
                                Console.WriteLine(md5Cache);
                            }                     ///////////////////////
                        }
                        // else fallout of loop no match
                    }
                }

                //  bool md5s
                if ((md5p7w == md5p7) && (md5Cache == md5ficache)) 
                { 
                    md5cacheFI = "FFIO match";
                    Console.WriteLine("FFIO md5 match");
                }

                return md5cacheFI;
            }

            //    Field Index   //
            void GenFieldIndex()
            {
                int[][] r2 = raddresses; int p7w = 0; int r7 = 0; int[] r3 = new int[7];
                // pbar_tree.Minimum = 0; pbar_tree.Value = 0; pbar_tree.Maximum = ff9Fields.Count;
                //richTextBox_debug.Text += "\nFI gen >";
                var t = Task.Run(async delegate { await Task.Delay(600); }); t.Wait();
                for (int fe = 0; fe < ff9Fields.Count; fe++)
                {
                    (int key, string value, byte[] asc2) = DictBytesConvert(fe);
                    r2 = raddresses; r3 = new int[7]; p7w = 0; r7 = 0;    //  only reset byte worker and ranges once during field
                    for (int p7 = 0; p7 < ba_p0data7.Length; p7++)    //  start/continue iteration using worker
                    {
                        if (p7 > deadzone[0] & p7 < deadzone[1]) { p7 = deadzone[1] + 1; }  //  deadzone check and pass
                        if (ba_p0data7[p7] == asc2[0])   //  first byte match
                        {
                            p7w = p7;   //  worker zone p7w
                            for (int fbl = 0; fbl < asc2.Length; fbl++)  //  iterate and match field ascii
                            {
                                if (!(ba_p0data7[p7w] == asc2[fbl])) { break; } //  break early if false match
                                if (ba_p0data7[p7w] == asc2[fbl])   //  found matching byte, worker++
                                {
                                    p7w++;
                                    if ((p7w - p7) == asc2.Length)   //  found match same length as the source pattern
                                    {
                                        r3[r7] = p7w;
                                        r7++;   //  thriple loop here not needed. we just r7++ untill end of file, always 7
                                    }
                                }
                            }
                            p7 = p7w;   //  leaving worker zone p7w   update p7 to last work address
                        }
                    }
                    //  add 7 r3 has all values, need to mux into r2
                    r2 = new int[][] { new int[] { r3[0], 0 }, new int[] { r3[1], 0 }, new int[] { r3[2], 0 }, new int[] { r3[3], 0 }, new int[] { r3[4], 0 }, new int[] { r3[5], 0 }, new int[] { r3[6], 0 } };
                    FieldIndices.Add(new FieldIndex(key, value, asc2, false, r2));  //  only create 1 with all r2[r7] data
                    //Console.WriteLine("r2[0] " + FieldIndices.Last().RangeStartStop[0][0] + " r2[1] " + FieldIndices.Last().RangeStartStop[1][0] + "");
                    //pbar_tree.Value = fe;
                }
                Console.WriteLine("> " + FieldIndices.Count + " generated");
                //CacheIO("write SB_FIO");                
            }
            void GenFinalFieldIndex()
            {
                int[] start = new int[5719];    //  FieldIndicies.Count * FieldIndicies.RangesStartStop.Length = 5719
                int k = 0; //pbar_tree.Value = 0; pbar_tree.Maximum = FieldIndices.Count;
                for (int i = 0; i < FieldIndices.Count; i++)
                {
                    for (int j = 0; j < FieldIndices[i].RangeStartStop.Length; j++)
                    {
                        start[k] = FieldIndices[i].RangeStartStop[j][0];
                        k++;
                    }
                }
                Array.Sort(start);
                for (int i = 0; i < FieldIndices.Count; i++)
                {
                    for (int j = 0; j < FieldIndices[i].RangeStartStop.Length; j++)
                    {
                        for (int kk = 0; kk < start.Length; kk++)
                        {
                            if (start[kk] == FieldIndices[i].RangeStartStop[j][0])
                            {
                                int end = 0;
                                if (kk == start.Length - 1) { end = ba_p0data7.Length; } //  to account for end file
                                else { end = start[kk + 1]; }   //  normal range end
                                FieldIndices[i].RangeStartStop[j][1] = end;
                            }
                        }
                    }
                    //pbar_tree.Value = i;
                }
                FinalFieldIndices = FieldIndices;
                CheckSumChecks("ffi");
                Console.WriteLine("FFI generated");
                CacheIO("write SB_FFIO");
            }
            //    Datapoints    //
            void FIDataset()
            {
                //pbar_tree.Minimum = 0; pbar_tree.Value = 0; pbar_tree.Maximum = FinalFieldIndices.Count;
                for (int ffi = 0; ffi < FinalFieldIndices.Count; ffi++) //  iterate through FFI (817)
                {
                    bool special = false; //    for pattern8 bypass lane
                    for (int ffir = 0; ffir < FinalFieldIndices[ffi].RangeStartStop.Length; ffir++) //  iterate through all ranges (7)
                    {
                        //  build new headers everytime the range changes (ffir)
                        List<int> headers = new List<int>();

                        for (int ffirs = FinalFieldIndices[ffi].RangeStartStop[ffir][0]; ffirs < FinalFieldIndices[ffi].RangeStartStop[ffir][1]; ffirs++)   //  scan range for headers before pattern searching
                        {
                            if (ba_p0data7[ffirs] == pattern6_init[0]) //  first byte of range init
                            {
                                //new pattern6 search
                                //match pattern 6 and 5 using corrected offsets
                                if ((ba_p0data7[ffirs] == pattern6_init[0]) && ((ba_p0data7[ffirs + 1] == pattern6_init[1]) | (ba_p0data7[ffirs + 1] == pattern6_BTNinit[1])) && (ba_p0data7[ffirs + 2] == pattern6_init[2]) && (ba_p0data7[ffirs + 3] == pattern6_init[3]) && ((ba_p0data7[ffirs + 4] == pattern6_init[4]) | (ba_p0data7[ffirs + 4] == pattern6_BTNinit[4])) && (ba_p0data7[ffirs + 5] == pattern6_init[5]) && (ba_p0data7[ffirs + 6] == pattern6_init[6]) && (ba_p0data7[ffirs + 7] == pattern6_init[7]) && ((ba_p0data7[ffirs + 10] == pattern6_init[10]) | (ba_p0data7[ffirs + 10] == pattern6_BTNinit[10]) | (ba_p0data7[ffirs + 10] == pattern6_404init[10])) && (ba_p0data7[ffirs + 11] == pattern6_init[11]))
                                {
                                    int i = BitConverter.ToUInt16(ba_p0data7, ffirs + 8);   //correct
                                                                                            //int j = (ba_p0data7[ffirs + 12] * 4); //weird
                                                                                            //int k = ffirs + 12 + j + 1; //pretty sure its right
                                                                                            //int l = (ffirs + 10 + i) - 9;   //  precise
                                    int k = (ffirs + i + 1);    //better
                                    if ((ba_p0data7[k] == pattern5_range[0]) && (ba_p0data7[k + 1] == pattern5_range[1]) && (ba_p0data7[k + 2] == pattern5_range[2]) && (ba_p0data7[k + 3] == pattern5_range[3]) && (ba_p0data7[k + 4] == pattern5_range[4]) && (ba_p0data7[k + 5] == pattern5_range[5]) && (ba_p0data7[k + 6] == pattern5_range[6]) && (ba_p0data7[k + 7] == pattern5_range[7]) && (ba_p0data7[k + 8] == pattern5_range[8]))
                                    {
                                        //  WindowAsync False Positive Catch    //
                                        //[] windowAsyncFP = new int[] { 44, 127, 32, 0, 1, 16, 38 }; // 2C 7F 20 00 01 10 26 WindowAsync False Positive in char loops
                                        bool a = false;
                                        for (int z = k; z < k+50; z++)  //  scan a little bit and search for Async
                                        {
                                            if (ba_p0data7[z] == windowAsyncFP[0] && ba_p0data7[z+1] == windowAsyncFP[1] && ba_p0data7[z+2] == windowAsyncFP[2] && ba_p0data7[z+3] == windowAsyncFP[3] && ba_p0data7[z+4] == windowAsyncFP[4] && ba_p0data7[z+5] == windowAsyncFP[5] && ba_p0data7[z+6] == windowAsyncFP[6])
                                            {
                                                ffirs = z + 6;
                                                a = true;
                                                //Console.WriteLine("sup2");
                                                break;
                                            }
                                        }
                                        if (!a)
                                        {
                                            // range match, log
                                            ffirs = k + 9;
                                            headers.Add(ffirs);
                                            //ffirs = (k + 9);
                                        } else
                                        {
                                            ffirs = k + 9;
                                        }
                                        //  WindowAsync False Positive Catch    //
                                    }
                                }
                            }
                        }
                        if ((FinalFieldIndices[ffi].FieldID == pattern8_noPreload[0]) || (FinalFieldIndices[ffi].FieldID == pattern8_noPreload[1]) || (FinalFieldIndices[ffi].FieldID == pattern8_noPreload[2]) || (FinalFieldIndices[ffi].FieldID == pattern8_noPreload[3]) || (FinalFieldIndices[ffi].FieldID == pattern8_noPreload[4]) || (FinalFieldIndices[ffi].FieldID == pattern8_noPreload[5]) || (FinalFieldIndices[ffi].FieldID == pattern8_noPreload[6]) || (FinalFieldIndices[ffi].FieldID == pattern8_noPreload[7]))
                        {
                            special = true;
                        }
                        for (int hc = 0; hc < headers.Count; hc++)  //  for each header, scanny scan
                        {
                            int hl = hc; bool hlast = false;
                            if (hc == headers.Count - 1)  //  last header
                            {
                                hl = FinalFieldIndices[ffi].RangeStartStop[ffir][1];
                                hlast = true;
                            }
                            if (hc < headers.Count - 1)   //  all but last header
                            {
                                hl = headers[hc + 1] - 38;  //  22+16 good enough
                                hlast = false;
                            }
                            int i = Dataset.Count;  //  log current dataset count
                            for (int ffirs = headers[hc]; ffirs < hl; ffirs++)  //  for each byte between headers
                            {
                                //  searching through index start and stops for 4 patterns in sequence
                                if (special || ba_p0data7[ffirs] == pattern1a_preload[0])  //  postive match on first byte
                                {
                                    ffirs = PatternFieldExits(ffi, ffir, ffirs, hc, hl, hlast, special);
                                    //  if return from fail, will cycle naturally to next value to start search
                                    //  if return from success, will continue search after positive match
                                }
                            }

                        }
                    }
                    //pbar_tree.Value = ffi;
                }
                //richTextBox_output.Text += "\nFin";
                int PatternFieldExits(int ffi, int ffir, int ffirs, int hc, int hl, bool hlast, bool special)
                {
                    if (special)
                    {   //special search
                        if ((ba_p0data7[ffirs] == pattern7_Tri127[0]) && (ba_p0data7[ffirs + 1] == pattern7_Tri127[1]) && (ba_p0data7[ffirs + 2] == pattern7_Tri127[2])) //  match triangles isntead
                        {
                            //positive match on p7_tri127   dummy data
                            pat1abytes = new byte[] { 0, 0 };
                            pat1a = new int[] { 0, 0 };
                            //match next pattern
                            if ((ba_p0data7[ffirs + 3] == pattern7_OpCea[0]) || ((ba_p0data7[ffirs + 3] == pattern7_OpCa9[0]) && (ba_p0data7[ffirs + 4] == pattern7_OpCa9[1]) && (ba_p0data7[ffirs + 5] == pattern7_OpCa9[2])))
                            {
                                //positive match on p7OpCodes   dummy data
                                pat1bbytes = new byte[] { 0, 0 };
                                pat1b = new int[] { 0, 0 };
                                int ffirs2 = ffirs + 6;   //  worker2 for new search updated to +6 for max opcode length
                                                          //next pattern may be lines away, need to continue search before next match.
                                for (ffirs2 = ffirs + 6; ffirs2 < hl; ffirs2++)   //  iterate through ba_p0data7 using indexes (/~/) range seasrch
                                {
                                    if (ba_p0data7[ffirs2] == pattern2a_setent[0])  //  postive match on first byte
                                    {

                                        if ((ba_p0data7[ffirs2] == pattern2a_setent[0]) && (ba_p0data7[ffirs2 + 1] == pattern2a_setent[1]) && (ba_p0data7[ffirs2 + 2] == pattern2a_setent[2]) && (ba_p0data7[ffirs2 + 3] == pattern2a_setent[3]))   //  match all
                                        {
                                            //positive match on p2a
                                            pat2abytes = new byte[] { ba_p0data7[ffirs2 + 4], ba_p0data7[(ffirs2 + 5)] };
                                            pat2a = new int[] { ffirs2 + 4, BitConverter.ToUInt16(ba_p0data7, ffirs2 + 4) };
                                            //match next pattern
                                            if ((ba_p0data7[ffirs2 + 6] == pattern2b_field[0]) && (ba_p0data7[ffirs2 + 7] == pattern2b_field[1]) && (ba_p0data7[ffirs2 + 8] == pattern2b_field[2]) && (ba_p0data7[ffirs2 + 9] == pattern2b_field[3]))   //  match all 
                                            {
                                                //positive match on p2b
                                                pat2bbytes = new byte[] { ba_p0data7[ffirs2 + 10], ba_p0data7[(ffirs2 + 11)] };
                                                pat2b = new int[] { ffirs2 + 10, BitConverter.ToUInt16(ba_p0data7, ffirs2 + 10) };
                                                int[][] dp_init = new int[][] { pat1a, pat1b, pat2a, pat2b };
                                                byte[][] dp_byte = new byte[][] { pat1abytes, pat1bbytes, pat2abytes, pat2bbytes };
                                                Dataset.Add(new Datapoint(FinalFieldIndices[ffi].FieldID, FinalFieldIndices[ffi].FieldName, false, false, ffir, hc, false, dp_init, dp_byte));
                                                if (Dataset.Count > 1)
                                                {
                                                    if ((Dataset[Dataset.Count - 1].FieldID == Dataset[Dataset.Count - 2].FieldID) && (Dataset[Dataset.Count - 1].RangeV == Dataset[Dataset.Count - 2].RangeV) && (Dataset[Dataset.Count - 1].HeaderC == Dataset[Dataset.Count - 2].HeaderC))   //  ifelse drop from list
                                                    {
                                                        Dataset.RemoveAt(Dataset.Count - 1);
                                                        Dataset.RemoveAt(Dataset.Count - 1);
                                                    }
                                                }
                                                ffirs2 += 12;
                                                if (hlast)
                                                {
                                                    if (ba_p0data7[ffirs2] == 4) // simple return exit following match
                                                    {
                                                        ffirs2 = hl;
                                                        return ffirs2;
                                                    }
                                                    //do small search for 2c 7f 04 return
                                                    for (int i = ffirs2; i < ffirs2 + 36; i++)
                                                    {
                                                        if ((ba_p0data7[i] == 4) && (ba_p0data7[i - 1] == 127) && (ba_p0data7[i - 2] == 44))
                                                        {
                                                            ffirs2 = hl;
                                                            return ffirs2;
                                                        }
                                                    }
                                                }
                                                return ffirs2; //return back to range search with address after positive match
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {   // regular search path
                        if ((ba_p0data7[ffirs] == pattern1a_preload[0]) && (ba_p0data7[ffirs + 1] == pattern1a_preload[1]) && (ba_p0data7[ffirs + 2] == pattern1a_preload[2])) //  match all
                        {
                            //positive match on p1a
                            pat1abytes = new byte[] { ba_p0data7[ffirs + 3], ba_p0data7[(ffirs + 4)] };
                            pat1a = new int[] { ffirs + 3, BitConverter.ToUInt16(ba_p0data7, ffirs + 3) };
                            //match next pattern
                            if ((ba_p0data7[ffirs + 5] == pattern1b_setvar[0]) && (ba_p0data7[ffirs + 6] == pattern1b_setvar[1]) && (ba_p0data7[ffirs + 7] == pattern1b_setvar[2]) && (rand9er.ba_p0data7[ffirs + 8] == pattern1b_setvar[3]))
                            {
                                //positive match on p1b
                                pat1bbytes = new byte[] { ba_p0data7[ffirs + 9], ba_p0data7[(ffirs + 10)] };
                                pat1b = new int[] { ffirs + 9, BitConverter.ToUInt16(ba_p0data7, ffirs + 9) };
                                int ffirs2 = ffirs + 11;   //  worker2 for new search updated to +11 for pattern p1a+varzone1+p1b+varzone2
                                                           //next pattern may be lines away, need to continue search before next match.
                                for (ffirs2 = ffirs + 11; ffirs2 < hl; ffirs2++)   //  iterate through ba_p0data7 using indexes (/~/) range seasrch        foreach range , now iterate through header address ranges      I think it was adding 11 twice, fixed now.
                                {
                                    //reminders
                                    //  pattern2a_setent = new byte[] { 5, 216, 2, 125 };    //var zone 2a y
                                    //  pattern2a2_d802 =  new byte[] { 5, 216, 2, 216, 2 };    //D802 x2 for double entrance
                                    //  pattern2a3_7e =    new byte[] { 5, 216, 2, 126 };      //7E for Long bytes (still only 2 bytes)
                                    //  pattern3_return = new byte[] { 4, 0 };                  // 04 00 = return of function
                                    //reminders

                                    if (ba_p0data7[ffirs2] == pattern2a_setent[0])  //  postive match on first byte
                                    {
                                        bool bool1 = (ba_p0data7[ffirs2] == pattern2a_setent[0]) && (ba_p0data7[ffirs2 + 1] == pattern2a_setent[1]) && (ba_p0data7[ffirs2 + 2] == pattern2a_setent[2]); //  0,1,2, first three match, all patterns share this
                                        bool bool2 = (ba_p0data7[ffirs2 + 3] == pattern2a_setent[3]);                                                                                                   //  offset 12 3, match setent, regular, grab next two bytes
                                        bool bool3 = (ba_p0data7[ffirs2 + 3] == pattern2a3_7e[3]);                                                                                                      //  offset 14 3, match 7e plus 2 [00] [00] bytes after, need to account for this
                                        bool bool4 = (ba_p0data7[ffirs2 + 3] == pattern2a2_d802[3]) && (ba_p0data7[ffirs2 + 4] == pattern2a2_d802[4]);                                                  //  offset 11 3,4, match d802 and store 3,4, losing a byte here "=" missing, need to account for this

                                        if (bool1 && (bool2 | bool3 | bool4))   //  bool1 has to match, and either 2 or 3 can match, or 4
                                        {
                                            if (bool2)  //  regular
                                            {
                                                //positive match on p2a
                                                pat2abytes = new byte[] { ba_p0data7[ffirs2 + 4], ba_p0data7[(ffirs2 + 5)] };
                                                pat2a = new int[] { ffirs2 + 4, BitConverter.ToUInt16(ba_p0data7, ffirs2 + 4) };

                                                //match next pattern
                                                if ((ba_p0data7[ffirs2 + 6] == pattern2b_field[0]) && (ba_p0data7[ffirs2 + 7] == pattern2b_field[1]) && (ba_p0data7[ffirs2 + 8] == pattern2b_field[2]) && (ba_p0data7[ffirs2 + 9] == pattern2b_field[3]))   //  match all 
                                                {
                                                    //positive match on p2b
                                                    pat2bbytes = new byte[] { ba_p0data7[ffirs2 + 10], ba_p0data7[(ffirs2 + 11)] };
                                                    pat2b = new int[] { ffirs2 + 10, BitConverter.ToUInt16(ba_p0data7, ffirs2 + 10) };
                                                    int[][] dp_init = new int[][] { pat1a, pat1b, pat2a, pat2b };
                                                    byte[][] dp_byte = new byte[][] { pat1abytes, pat1bbytes, pat2abytes, pat2bbytes };
                                                    Dataset.Add(new Datapoint(FinalFieldIndices[ffi].FieldID, FinalFieldIndices[ffi].FieldName, false, false, ffir, hc, false, dp_init, dp_byte));
                                                    if (Dataset.Count > 1)
                                                    {
                                                        if ((Dataset[Dataset.Count - 1].FieldID == Dataset[Dataset.Count - 2].FieldID) && (Dataset[Dataset.Count - 1].RangeV == Dataset[Dataset.Count - 2].RangeV) && (Dataset[Dataset.Count - 1].HeaderC == Dataset[Dataset.Count - 2].HeaderC))   //  ifelse drop from list
                                                        {
                                                            Dataset.RemoveAt(Dataset.Count - 1);
                                                            Dataset.RemoveAt(Dataset.Count - 1);
                                                        }
                                                    }
                                                    ffirs2 += 12;
                                                    if (hlast)
                                                    {
                                                        if (ba_p0data7[ffirs2] == 4) // simple return exit following match
                                                        {
                                                            ffirs2 = hl;
                                                            return ffirs2;
                                                        }
                                                        //do small search for 2c 7f 04 return
                                                        for (int i = ffirs2; i < ffirs2 + 36; i++)
                                                        {
                                                            if ((ba_p0data7[i] == 4) && (ba_p0data7[i - 1] == 127) && (ba_p0data7[i - 2] == 44))
                                                            {
                                                                ffirs2 = hl;
                                                                return ffirs2;
                                                            }
                                                        }
                                                    }
                                                    //richTextBox_output.Text += "\nmulti-header-7d";
                                                    return ffirs2; //return back to range search with address after positive match
                                                }
                                            }
                                            if (bool3)  //  7e  same as regular, but need to skip 2 extra bytes after gathering data, +2 total length later
                                            {
                                                //positive match on p2a
                                                pat2abytes = new byte[] { ba_p0data7[ffirs2 + 4], ba_p0data7[(ffirs2 + 5)] };
                                                pat2a = new int[] { ffirs2 + 4, BitConverter.ToUInt16(ba_p0data7, ffirs2 + 4) };

                                                //match next pattern
                                                if ((ba_p0data7[ffirs2 + 8] == pattern2b_field[0]) && (ba_p0data7[ffirs2 + 9] == pattern2b_field[1]) && (ba_p0data7[ffirs2 + 10] == pattern2b_field[2]) && (ba_p0data7[ffirs2 + 11] == pattern2b_field[3]))   //  match all
                                                {
                                                    //positive match on p2b
                                                    pat2bbytes = new byte[] { ba_p0data7[ffirs2 + 12], ba_p0data7[(ffirs2 + 13)] };
                                                    pat2b = new int[] { ffirs2 + 12, BitConverter.ToUInt16(ba_p0data7, ffirs2 + 12) };
                                                    int[][] dp_init = new int[][] { pat1a, pat1b, pat2a, pat2b };
                                                    byte[][] dp_byte = new byte[][] { pat1abytes, pat1bbytes, pat2abytes, pat2bbytes };
                                                    Dataset.Add(new Datapoint(FinalFieldIndices[ffi].FieldID, FinalFieldIndices[ffi].FieldName, false, false, ffir, hc, false, dp_init, dp_byte));
                                                    if (Dataset.Count > 1)
                                                    {
                                                        if ((Dataset[Dataset.Count - 1].FieldID == Dataset[Dataset.Count - 2].FieldID) && (Dataset[Dataset.Count - 1].RangeV == Dataset[Dataset.Count - 2].RangeV) && (Dataset[Dataset.Count - 1].HeaderC == Dataset[Dataset.Count - 2].HeaderC))   //  ifelse drop from list
                                                        {
                                                            Dataset.RemoveAt(Dataset.Count - 1);
                                                            Dataset.RemoveAt(Dataset.Count - 1);
                                                        }
                                                    }
                                                    ffirs2 += 14;
                                                    if (hlast)
                                                    {
                                                        if (ba_p0data7[ffirs2] == 4) // simple return exit following match
                                                        {
                                                            ffirs2 = hl;
                                                            return ffirs2;
                                                        }
                                                        //do small search for 2c 7f 04 return
                                                        for (int i = ffirs2; i < ffirs2 + 36; i++)
                                                        {
                                                            if ((ba_p0data7[i] == 4) && (ba_p0data7[i - 1] == 127) && (ba_p0data7[i - 2] == 44))
                                                            {
                                                                ffirs2 = hl;
                                                                return ffirs2;
                                                            }
                                                        }
                                                    }
                                                    //richTextBox_output.Text += "\nmulti-header-7e";
                                                    return ffirs2; //return back to range search with address after positive match
                                                }
                                            }
                                            if (bool4)  //  d802    start gathering one byte prior to regular, one less total length later
                                            {
                                                //positive match on p2a
                                                pat2abytes = new byte[] { ba_p0data7[ffirs2 + 3], ba_p0data7[(ffirs2 + 4)] };
                                                pat2a = new int[] { ffirs2 + 3, BitConverter.ToUInt16(ba_p0data7, ffirs2 + 3) };

                                                //match next pattern
                                                if ((rand9er.ba_p0data7[ffirs2 + 5] == pattern2b_field[0]) && (ba_p0data7[ffirs2 + 6] == pattern2b_field[1]) && (ba_p0data7[ffirs2 + 7] == pattern2b_field[2]) && (ba_p0data7[ffirs2 + 8] == pattern2b_field[3]))   //  match all
                                                {
                                                    //positive match on p2b
                                                    pat2bbytes = new byte[] { ba_p0data7[ffirs2 + 9], ba_p0data7[(ffirs2 + 10)] };
                                                    pat2b = new int[] { ffirs2 + 9, BitConverter.ToUInt16(ba_p0data7, ffirs2 + 9) };
                                                    int[][] dp_init = new int[][] { pat1a, pat1b, pat2a, pat2b };
                                                    byte[][] dp_byte = new byte[][] { pat1abytes, pat1bbytes, pat2abytes, pat2bbytes };
                                                    Dataset.Add(new Datapoint(FinalFieldIndices[ffi].FieldID, FinalFieldIndices[ffi].FieldName, false, false, ffir, hc, false, dp_init, dp_byte));
                                                    if (Dataset.Count > 1)
                                                    {
                                                        if ((Dataset[Dataset.Count - 1].FieldID == Dataset[Dataset.Count - 2].FieldID) && (Dataset[Dataset.Count - 1].RangeV == Dataset[Dataset.Count - 2].RangeV) && (Dataset[Dataset.Count - 1].HeaderC == Dataset[Dataset.Count - 2].HeaderC))   //  ifelse drop from list
                                                        {
                                                            Dataset.RemoveAt(Dataset.Count - 1);
                                                            Dataset.RemoveAt(Dataset.Count - 1);
                                                        }
                                                    }
                                                    ffirs2 += 11;
                                                    if (hlast)
                                                    {
                                                        if (ba_p0data7[ffirs2] == 4) // simple return exit following match
                                                        {
                                                            ffirs2 = hl;
                                                            return ffirs2;
                                                        }
                                                        //do small search for 2c 7f 04 return
                                                        for (int i = ffirs2; i < ffirs2 + 36; i++)
                                                        {
                                                            if ((ba_p0data7[i] == 4) && (ba_p0data7[i - 1] == 127) && (ba_p0data7[i - 2] == 44))
                                                            {
                                                                ffirs2 = hl;
                                                                return ffirs2;
                                                            }
                                                        }
                                                    }
                                                    //richTextBox_output.Text += "\nmulti-header-d802d802";
                                                    return ffirs2; //return back to range search with address after positive match
                                                }
                                            }

                                        }
                                    }
                                }
                            }
                        }
                    }
                    return ffirs;  //  return back to range search with current address to failing match
                }


            }
            //    ReArrange     //
            void MFIgen()
            {
                for (int fi = 0; fi < ff9Fields.Count; fi++)
                {
                    int fid = ff9Fields.ElementAt(fi).Key; string fname = ff9Fields.ElementAt(fi).Value;
                    MainFieldIndex.Add(new FieldData(fid, fname, "", new List<int>(), new List<int[]>(), new List<int[]>(), new List<int>(), new List<int>(), new List<int>()));   //  create new DP2 at first datapoint
                    DataSIMP = new List<Datapoint>(Dataset);    //  create new copy of list each loop
                    DataSIMP.RemoveAll(datapoint => !(datapoint.FieldID == fid));

                    if (DataSIMP.Count > 0)
                    {
                        MFIsimp();
                        void MFIsimp()
                        {
                            int exitNum = DataSIMP[0].HeaderC;
                            int[] exitAddress = new int[21];    //  
                            int[] genAddress = new int[7];     //  same
                            int i = 0; int j = 0; int exitValue = 0; int genValue = 0;
                            for (int si = 0; si < DataSIMP.Count; si++)    //  iterate all list and log data
                            {
                                bool temp31b = (DataSIMP[si].AddressNvalue[3][1] == DataSIMP[0].AddressNvalue[3][1]);   //exit fid check
                                bool temp21b = (DataSIMP[si].AddressNvalue[2][1] == DataSIMP[0].AddressNvalue[2][1]);   //exit gen check
                                bool tempheadb = (DataSIMP[si].HeaderC == DataSIMP[0].HeaderC); //exit header check
                                if (temp31b && temp21b && tempheadb) //  match both exitfid and gen of first in list.
                                {
                                    //store current line data into worker arrays
                                    exitAddress[i] = DataSIMP[si].AddressNvalue[0][0];
                                    exitAddress[i + 1] = DataSIMP[si].AddressNvalue[1][0];
                                    exitAddress[i + 2] = DataSIMP[si].AddressNvalue[3][0];
                                    i += 3;
                                    genAddress[j] = DataSIMP[si].AddressNvalue[2][0];
                                    j += 1;
                                    exitValue = DataSIMP[si].AddressNvalue[3][1];
                                    genValue = DataSIMP[si].AddressNvalue[2][1];
                                }
                            }
                            //  add total worker to last data
                            MainFieldIndex.Last().ExitAddress.Add(exitAddress);
                            MainFieldIndex.Last().GenAddress.Add(genAddress);
                            MainFieldIndex.Last().ExitValue.Add(exitValue);
                            MainFieldIndex.Last().GenValue.Add(genValue);
                            MainFieldIndex.Last().ExitNum.Add(exitNum);

                            DataSIMP.RemoveAll(datapoint => datapoint.HeaderC == exitNum);  //  remove logged data from current list by means of headerC
                            if (DataSIMP.Count > 0)
                            {
                                MFIsimp();
                            }
                        }
                    }

                }
                //  now we have MFI ordered much better, output and verify
            }
            void MFIgv()    //  add all GE's to source field they exit to.
            {
                for (int i = 0; i < MainFieldIndex.Count; i++)
                {
                    for (int j = 0; j < MainFieldIndex[i].ExitValue.Count; j++)
                    {
                        for (int k = 0; k < MainFieldIndex.Count; k++)
                        {
                            if (MainFieldIndex[k].FieldID == MainFieldIndex[i].ExitValue.ElementAt(j))
                            {
                                MainFieldIndex[k].AllGE.Add(MainFieldIndex[i].GenValue.ElementAt(j));
                            }
                        }
                    }
                }


            }

            void MFIroomType()
            {
                
                for (int i = 0; i < MainFieldIndex.Count; i++)
                {
                    if (MainFieldIndex[i].ExitValue.Count == 1)
                    {
                        //this is a room with 1 exit CLOSET
                    }
                    if (MainFieldIndex[i].ExitValue.Count == 2)
                    {
                        //this is a room with 2 exit HALLWAY
                    }
                    if (MainFieldIndex[i].ExitValue.Count == 3)
                    {
                        //this is a room with 3 exit ROOM
                    }
                    if (MainFieldIndex[i].ExitValue.Count == 3)
                    {
                        //this is a room with 3 exit ROOM
                    }
                    if (MainFieldIndex[i].ExitValue.Count > 3)
                    {
                        //this is a room with 3 exit HUB
                    }


                    
                }
            }


            //      END Funcs       //

            bLogic();

            void bLogic()
            {
                //  cache
                CacheCheck();   //  md5 checks, restore cache from inside if needed, generate cache as fallback
                                //  FFIO created!, verified with some testing, spot checked output. quick output all rstored 

                //  scan and create Dataset<datapoint>
                FIDataset();

                //  re arrange
                //MFIgen();

                //  rng time
                //Rubiks();

                //  output dataset for testing

                void DSout()
                {
                    string temp = ""; string[] temp2 = new string[] { temp, "" };
                    int count = 1; string temp4 = ""; int[] temp5 = new int[] { 0, 0, 0, 0, 0, 0, 0 };
                    for (int i = 0; i < Dataset.Count; i++)
                    {
                        Dataset[i].SumCheck(); Dataset[i].SumCheckBytes();
                        if (i > 0)
                        {
                            if (Dataset[i].FieldID == Dataset[i - 1].FieldID)
                            {
                                if (!(Dataset[i].RangeV == Dataset[i - 1].RangeV))
                                {
                                    //if (!(Dataset[i].HeaderC == 0))
                                    //{
                                    //    richTextBox_output.Text += "\n skipped one";
                                    //}
                                }
                                if (!(Dataset[i].HeaderC == Dataset[i - 1].HeaderC))
                                {
                                    temp += "DSID=" + Dataset[i].FieldID + " r=" + Dataset[i].RangeV + " CheckSum=" + Dataset[i].CheckSum + " headerC=" + Dataset[i].HeaderC + " 00=" + Dataset[i].AddressNvalue[0][0] + " 01=" + Dataset[i].AddressNvalue[0][1] + " 10=" + Dataset[i].AddressNvalue[1][0] + " 11=" + Dataset[i].AddressNvalue[1][1] + " 20=" + Dataset[i].AddressNvalue[2][0] + " 21=" + Dataset[i].AddressNvalue[2][1] + " 30=" + Dataset[i].AddressNvalue[3][0] + " 31=" + Dataset[i].AddressNvalue[3][1] + "\n";

                                }
                            }
                            else
                            {
                                //if (!(Dataset[i].HeaderC == 0))
                                //{
                                //    richTextBox_output.Text += "\n skipped one";
                                //}
                                temp += "DSID=" + Dataset[i].FieldID + " r=" + Dataset[i].RangeV + " CheckSum=" + Dataset[i].CheckSum + " headerC=" + Dataset[i].HeaderC + " 00=" + Dataset[i].AddressNvalue[0][0] + " 01=" + Dataset[i].AddressNvalue[0][1] + " 10=" + Dataset[i].AddressNvalue[1][0] + " 11=" + Dataset[i].AddressNvalue[1][1] + " 20=" + Dataset[i].AddressNvalue[2][0] + " 21=" + Dataset[i].AddressNvalue[2][1] + " 30=" + Dataset[i].AddressNvalue[3][0] + " 31=" + Dataset[i].AddressNvalue[3][1] + "\n";

                            }


                        }
                        else
                        {
                            temp += "DSID=" + Dataset[i].FieldID + " r=" + Dataset[i].RangeV + " CheckSum=" + Dataset[i].CheckSum + " headerC=" + Dataset[i].HeaderC + " 00=" + Dataset[i].AddressNvalue[0][0] + " 01=" + Dataset[i].AddressNvalue[0][1] + " 10=" + Dataset[i].AddressNvalue[1][0] + " 11=" + Dataset[i].AddressNvalue[1][1] + " 20=" + Dataset[i].AddressNvalue[2][0] + " 21=" + Dataset[i].AddressNvalue[2][1] + " 30=" + Dataset[i].AddressNvalue[3][0] + " 31=" + Dataset[i].AddressNvalue[3][1] + "\n";

                        }


                        //temp += "DSID=" + Dataset[i].FieldID + " r=" + Dataset[i].RangeV + " CheckSum=" + Dataset[i].CheckSum + " headerC=" + Dataset[i].HeaderC + " 00=" + Dataset[i].AddressNvalue[0][0] + " 01=" + Dataset[i].AddressNvalue[0][1] + " 10=" + Dataset[i].AddressNvalue[1][0] + " 11=" + Dataset[i].AddressNvalue[1][1] + " 20=" + Dataset[i].AddressNvalue[2][0] + " 21=" + Dataset[i].AddressNvalue[2][1] + " 30=" + Dataset[i].AddressNvalue[3][0] + " 31=" + Dataset[i].AddressNvalue[3][1] + "\n";
                        //temp += "DSID=" + Dataset[i].FieldID + " r=" + Dataset[i].RangeV + " CheckSum=" + Dataset[i].CheckSum + " 00=" + Dataset[i].ValueBytes[0][0] + " 01=" + Dataset[i].ValueBytes[0][1] + " 10=" + Dataset[i].ValueBytes[1][0] + " 11=" + Dataset[i].ValueBytes[1][1] + " 20=" + Dataset[i].ValueBytes[2][0] + " 21=" + Dataset[i].ValueBytes[2][1] + " 30=" + Dataset[i].ValueBytes[3][0] + " 31=" + Dataset[i].ValueBytes[3][1] + "\n";

                    }
                    temp2 = new string[] { temp, "" }; //  using right now
                    Directory.SetCurrentDirectory("C:\\Users\\user\\Desktop\\");
                    File.WriteAllLines("DSlite.txt", temp2);
                    temp = ""; temp2 = new string[] { "", "" };
                }
                void DSoutFull()
                {
                    string temp = ""; string[] temp2 = new string[] { temp, "" };
                    int count = 1; string temp4 = ""; int[] temp5 = new int[] { 0, 0, 0, 0, 0, 0, 0 };
                    for (int i = 0; i < Dataset.Count; i++)
                    {
                        Dataset[i].SumCheck(); Dataset[i].SumCheckBytes();
                        temp += "DSID=" + Dataset[i].FieldID + " r=" + Dataset[i].RangeV + " CheckSum=" + Dataset[i].CheckSum + " headerC=" + Dataset[i].HeaderC + " 00=" + Dataset[i].AddressNvalue[0][0] + " 01=" + Dataset[i].AddressNvalue[0][1] + " 10=" + Dataset[i].AddressNvalue[1][0] + " 11=" + Dataset[i].AddressNvalue[1][1] + " 20=" + Dataset[i].AddressNvalue[2][0] + " 21=" + Dataset[i].AddressNvalue[2][1] + " 30=" + Dataset[i].AddressNvalue[3][0] + " 31=" + Dataset[i].AddressNvalue[3][1] + "\n";

                    }
                    temp2 = new string[] { temp, "" }; //  using right now
                    Directory.SetCurrentDirectory("C:\\Users\\user\\Desktop\\");
                    File.WriteAllLines("DSfull.txt", temp2);
                    temp = ""; temp2 = new string[] { "", "" };
                }

                void MFIoutFull()
                {
                    string temp = ""; string[] temp2 = new string[] { temp, "" };
                    int count = 1; string temp4 = ""; int[] temp5 = new int[] { 0, 0, 0, 0, 0, 0, 0 };
                    for (int i = 0; i < MainFieldIndex.Count; i++)
                    {
                        string ea = "";
                        for (int j = 0; j < MainFieldIndex[i].ExitAddress.Count; j++)
                        {
                            for (int k = 0; k < MainFieldIndex[i].ExitAddress.ElementAt(j).Length; k++)
                            {
                                ea += j + "=" + MainFieldIndex[i].ExitAddress.ElementAt(j)[k] + " ";
                            }
                        }
                        string ga = "";
                        for (int j = 0; j < MainFieldIndex[i].GenAddress.Count; j++)
                        {
                            for (int k = 0; k < MainFieldIndex[i].GenAddress.ElementAt(j).Length; k++)
                            {
                                ga += j + "=" + MainFieldIndex[i].GenAddress.ElementAt(j)[k] + " ";
                            }
                        }
                        string allge = "";
                        for (int j = 0; j < MainFieldIndex[i].AllGE.Count; j++)
                        {
                            allge += j + "=" + MainFieldIndex[i].AllGE[j] + " ";
                        }
                        string ev = "";
                        for (int j = 0; j < MainFieldIndex[i].ExitValue.Count; j++)
                        {
                            ev += j + "=" + MainFieldIndex[i].ExitValue[j] + " ";
                        }
                        string gv = "";
                        for (int j = 0; j < MainFieldIndex[i].GenValue.Count; j++)
                        {
                            gv += j + "=" + MainFieldIndex[i].GenValue[j] + " ";
                        }
                        temp += "MFId=" + MainFieldIndex[i].FieldID + " exit=" + ev + " gen=" + gv + " allge=" + allge + " eAdds=" + ea + " gAdds=" + ga + "\n";
                    }
                    temp2 = new string[] { temp, "" }; //  using right now
                    Directory.SetCurrentDirectory("C:\\Users\\user\\Desktop\\");
                    File.WriteAllLines("MFIFull.txt", temp2);
                    temp = ""; temp2 = new string[] { "", "" };
                }
                void MFIoutLite()
                {
                    string temp = ""; string[] temp2 = new string[] { temp, "" };
                    int count = 1; string temp4 = ""; int[] temp5 = new int[] { 0, 0, 0, 0, 0, 0, 0 };
                    for (int i = 0; i < MainFieldIndex.Count; i++)
                    {
                        string ea = "";
                        for (int j = 0; j < MainFieldIndex[i].ExitAddress.Count; j++)
                        {
                            for (int k = 0; k < MainFieldIndex[i].ExitAddress.ElementAt(j).Length; k++)
                            {
                                ea += j + "=" + MainFieldIndex[i].ExitAddress.ElementAt(j)[k] + " ";
                            }
                        }
                        string ga = "";
                        for (int j = 0; j < MainFieldIndex[i].GenAddress.Count; j++)
                        {
                            for (int k = 0; k < MainFieldIndex[i].GenAddress.ElementAt(j).Length; k++)
                            {
                                ga += j + "=" + MainFieldIndex[i].GenAddress.ElementAt(j)[k] + " ";
                            }
                        }
                        string allge = "";
                        for (int j = 0; j < MainFieldIndex[i].AllGE.Count; j++)
                        {
                            allge += j + "=" + MainFieldIndex[i].AllGE[j] + " ";
                        }
                        string ev = "";
                        for (int j = 0; j < MainFieldIndex[i].ExitValue.Count; j++)
                        {
                            ev += j + "=" + MainFieldIndex[i].ExitValue[j] + " ";
                        }
                        string gv = "";
                        for (int j = 0; j < MainFieldIndex[i].GenValue.Count; j++)
                        {
                            gv += j + "=" + MainFieldIndex[i].GenValue[j] + " ";
                        }
                        string rt = "";
                        rt += MainFieldIndex[i].ExitNum.Count + " ";
                        temp += "MFId=" + MainFieldIndex[i].FieldID + " exit=" + ev + " gen=" + gv + " allge=" + allge + " rt="+ rt + "\n";
                    }
                    temp2 = new string[] { temp, "" }; //  using right now
                    Directory.SetCurrentDirectory("C:\\Users\\user\\Desktop\\");
                    File.WriteAllLines("MFIlite.txt", temp2);
                    temp = ""; temp2 = new string[] { "", "" };
                }
                /*void MFIoutroomSize()
                {
                    string temp = ""; string[] temp2 = new string[] { temp, "" };
                    int count = 1; string temp4 = ""; int[] temp5 = new int[] { 0, 0, 0, 0, 0, 0, 0 };
                    for (int i = 0; i < MainFieldIndex.Count; i++)
                    {
                        string rt = "";
                        rt += MainFieldIndex[i].ExitNum.Count + " ";
                        if (MainFieldIndex[i].ExitNum.Count > 4)
                        temp += "MFId=" + MainFieldIndex[i].FieldID + " rt="+ rt + "\n";
                    }
                    temp2 = new string[] { temp, "" }; //  using right now
                    Directory.SetCurrentDirectory("C:\\Users\\user\\Desktop\\");
                    File.WriteAllLines("MFIroomSize5p.txt", temp2);
                    temp = ""; temp2 = new string[] { "", "" };
                    int ccount = 0;
                    for (int i = 0; i < CityArr.Count; i++)
                    {
                        if (CityArr[i][1] > 0)
                        {
                            ccount++;
                        }
                    }
                    
                    Console.WriteLine(ccount + " Cities");
                }*/
                void RangeVCheck()
                {
                    string temp = ""; string[] temp2 = new string[] { temp, "" };
                    int count = 1; string temp4 = ""; int[] temp5 = new int[] { 0, 0, 0, 0, 0, 0, 0 };
                    for (int i = 0; i < Dataset.Count; i++)
                    {

                        if (i > 0)
                        {
                            //count r=n per field id
                            if ((Dataset[i].FieldID == Dataset[i - 1].FieldID) && (Dataset[i].RangeV == Dataset[i - 1].RangeV))
                            {
                                count++;
                            }
                            else if ((Dataset[i].FieldID == Dataset[i - 1].FieldID) && (Dataset[i].RangeV > Dataset[i - 1].RangeV))
                            {
                                //temp4 += "F=" + Dataset[i - 1].FieldID + " R=" + Dataset[i - 1].RangeV + " exitCount=" + count + "\n";
                                temp5[Dataset[i - 1].RangeV] = count;
                                count = 1;

                            }
                            else if (!(Dataset[i].FieldID == Dataset[i - 1].FieldID))
                            {
                                temp5[Dataset[i - 1].RangeV] = count;
                                bool temp6 = (temp5[0] == temp5[1] && temp5[0] == temp5[2] && temp5[0] == temp5[3] && temp5[0] == temp5[4] && temp5[0] == temp5[5] && temp5[0] == temp5[6]);
                                temp += "F=" + Dataset[i - 1].FieldID + " language range bool=" + temp6 + "\n";
                                count = 1;

                            }

                            if ((i == Dataset.Count - 1) && (rand9er.counter > 1)) //   last run
                            {
                                temp5[Dataset[i - 1].RangeV] = count;
                                bool temp6 = (temp5[0] == temp5[1] && temp5[0] == temp5[2] && temp5[0] == temp5[3] && temp5[0] == temp5[4] && temp5[0] == temp5[5] && temp5[0] == temp5[6]);
                                temp += "F=" + Dataset[i - 1].FieldID + " language range bool=" + temp6 + "\n";
                                count = 1;
                            }
                        }

                    }
                    temp2 = new string[] { temp, "" }; //  using right now
                    Directory.SetCurrentDirectory("C:\\Users\\drew\\Desktop\\Testdata\\");
                    File.WriteAllLines("DSfull.txt", temp2);
                    temp = ""; temp2 = new string[] { "", "" };

                }
                var instance = new Logic();
                instance.LogicOut();
                DSout();
                //  testing output with this func
                //RangeVCheck();
                DSoutFull();
                //DS3out();
                //  output to txt file
                MFIgen();
                MFIgv();
                MFIoutFull();
                MFIoutLite();
                //MFIoutroomSize();
                //  connect all exits to verify full map match
                Mapp();

                //  math to scramble
                //Rubiks();

                //  write bin
                Ink();

                void Testing()
                {

                    //      LINES OUTPUT TEST CODE


                    /*
                string temp = ""; string[] temp2 = new string[] { temp, "" };
                for (int i = 0; i < FinalFieldIndices.Count; i++)
                {
                    FinalFieldIndices[i].SumCheck();
                    temp += "\nFFID= " + FinalFieldIndices[i].FieldID + " CheckSum= " + FinalFieldIndices[i].CheckSum + " 00=" + FinalFieldIndices[i].RangeStartStop[0][0] + " 01=" + FinalFieldIndices[i].RangeStartStop[0][1] + " 10=" + FinalFieldIndices[i].RangeStartStop[1][0] + " 11=" + FinalFieldIndices[i].RangeStartStop[1][1] + " 20=" + FinalFieldIndices[i].RangeStartStop[2][0] + " 21=" + FinalFieldIndices[i].RangeStartStop[2][1] + " 30=" + FinalFieldIndices[i].RangeStartStop[3][0] + " 31=" + FinalFieldIndices[i].RangeStartStop[3][1] + " 40=" + FinalFieldIndices[i].RangeStartStop[4][0] + " 41=" + FinalFieldIndices[i].RangeStartStop[4][1] + " 50=" + FinalFieldIndices[i].RangeStartStop[5][0] + " 51=" + FinalFieldIndices[i].RangeStartStop[5][1] + " 60=" + FinalFieldIndices[i].RangeStartStop[6][0] + " 61=" + FinalFieldIndices[i].RangeStartStop[6][1] + "\n";

                }
                temp2 = new string[] { temp, "" };
                Directory.SetCurrentDirectory("C:\\Users\\drew\\Desktop\\Testdata\\");
                File.WriteAllLines("Final Field Indexes restiored from cache object inside program.txt", temp2);
                temp = ""; temp2 = new string[] { "", "" };
                */


                    /*
                    string temp = ""; string[] temp2 = new string[] { temp, "" };
                    for (int i = 0; i < FinalFieldIndices.Count; i++)
                    { temp += "FFI: fID= " + FinalFieldIndices[i].FieldID + " CheckSum= " + FinalFieldIndices[i].CheckSum; }
                    temp2 = new string[] { temp, "" };
                    Directory.SetCurrentDirectory("C:\\Users\\drew\\Desktop\\Testdata\\");
                    File.WriteAllLines("Final Field Indexes.txt", temp2);                
                    temp = ""; temp2 = new string[] { "", "" };
                    */

                    //CacheIO("read SB_FFIO");
                    //GenFieldIndex();
                    //GenFinalFieldIndex();
                    /*
                    using (var md5 = MD5.Create())
                    {
                        string md52;
                        string dirTest = "\\SB_FFIO_mog.bin";
                        //  md5 p0data7.bin
                        using (var stream = File.OpenRead(tb_fl.Text + dirTest))
                        { md52 = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant(); }
                        richTextBox_output.Text += "\n" + md52;
                    }
                    */
                    //richTextBox_output.Text += "\n" + md52;
                    /*
                    if (FinalFieldIndices.Count > 0)
                    {
                        richTextBox_output.Text += "\nFFI count= " + FinalFieldIndices.Count + " spot " + FinalFieldIndices[500].RangeStartStop[3][1];

                    }
                    */
                }

            }

            void Mapp()
            {
                // here we try to build connections using DS
                /*              
                 *              draw a box
                 */

            }


            void Ink()
            {

            }

            //      end of Bytes_IO()
        }
    }
}

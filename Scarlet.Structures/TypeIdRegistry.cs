namespace Scarlet.Structures;

public static class TypeIdRegistry {
    public static Dictionary<uint, string> IdTable { get; set; } = new() {
        { 0x66DBDu, "1024.heb" },
        { 0x193F0u, "2048.heb" },
        { 0x1D37Du, "256.heb" },
        { 0xA96B1u, "2dcol" },
        { 0xDE1ADu, "4096.heb" },
        { 0xC1010u, "5.ccb" },
        { 0x608D4u, "512.heb" },
        { 0xCBC81u, "acd" },
        { 0x99DCEu, "aig" },
        { 0x676E7u, "aiia" },
        { 0xECC50u, "aiia_erudition.xml" },
        { 0xC18DEu, "aiia.dbg" },
        { 0xFD0B4u, "aiia.ref" },
        { 0x7A4B2u, "alist" },
        { 0xA5AB9u, "amdl" },
        { 0x4F423u, "ani" },
        { 0x56E70u, "anmgph" },
        { 0xDD5A0u, "apb" },
        { 0xE01CEu, "apx" },
        { 0xA68CAu, "atlas" },
        { 0x5C91Bu, "autoext" },
        { 0xCC79Fu, "bcfg" },
        { 0xA31A0u, "bin" },
        { 0x2A989u, "bins" },
        { 0x38FF3u, "blast" },
        { 0xCF1B0u, "blastc" },
        { 0x3B24Fu, "blos" },
        { 0x474C2u, "bmdl" },
        { 0x28B3Eu, "bnm" },
        { 0x8F081u, "bnmwnd" },
        { 0xEC2B0u, "bod" },
        { 0x259C8u, "btex" },
        { 0xA806Du, "btexheader" },
        { 0xEB241u, "ccb" },
        { 0xEB90Du, "ccf" },
        { 0x9ACD3u, "ch.sb" },
        { 0xA8C41u, "clsn" },
        { 0xA7477u, "clsx" },
        { 0x7B3BFu, "cmdl" },
        { 0xFA8B7u, "color.btex" },
        { 0xB925u, "color.dds.bcfg" },
        { 0x32601u, "config" },
        { 0x3D6D8u, "customdata" },
        { 0x38AC8u, "dat" },
        { 0x49D2Bu, "data" },
        { 0xD1642u, "db3" },
        { 0xD6106u, "dbg" },
        { 0xB1960u, "dds" },
        { 0x80D26u, "dds.bcfg" },
        { 0x80CF0u, "dearc" },
        { 0xFD5BCu, "dml" },
        { 0x393A3u, "dummy.txt" },
        { 0xE3850u, "dx11.fntbin" },
        { 0x9A5EEu, "dyn" },
        { 0x2DCA6u, "earc" },
        { 0x3CF07u, "earcref" },
        { 0xDC0F5u, "ebex" },
        { 0xFB046u, "elx" },
        { 0x9276Fu, "emem" },
        { 0xF072Eu, "empath" },
        { 0xAA358u, "enttray" },
        { 0x2E1FDu, "erep" },
        { 0xE5A3Fu, "exml" },
        { 0x5FB44u, "exr" },
        { 0x16D72u, "file_list" },
        { 0xE8C2Eu, "fntbin" },
        { 0x190C9u, "folg" },
        { 0xB0B32u, "folgbin" },
        { 0xEF4DBu, "gmdl" },
        { 0xD1078u, "gmdl_hair" },
        { 0xA2343u, "gmdl.gfxbin" },
        { 0x12F0Bu, "gmtl" },
        { 0xF2DB3u, "gmtl.gfxbin" },
        { 0x3911Eu, "gmtla" },
        { 0xC5E90u, "gpubin" },
        { 0xDD95Au, "heb" },
        { 0x3108Cu, "hephysx" },
        { 0x6E602u, "high.folgbin" },
        { 0xC2CC2u, "htpk" },
        { 0x586A5u, "id2s" },
        { 0x70F36u, "ies" },
        { 0xEC2F6u, "ikts" },
        { 0xBC182u, "irr" },
        { 0x44D11u, "json" },
        { 0x9473Bu, "kab" },
        { 0x196A7u, "kdi" },
        { 0x36D11u, "layer_info" },
        { 0x64FA4u, "layer_pcd" },
        { 0xD9035u, "lik" },
        { 0x677CAu, "lipmap" },
        { 0x571EBu, "list" },
        { 0xFEBCBu, "listb" },
        { 0x37D88u, "lnkani" },
        { 0xEF836u, "low.tmsbin" },
        { 0xB064Au, "lsd" },
        { 0xC830u, "mask.btex" },
        { 0x41EEu, "mask.dds.bcfg" },
        { 0x7A08Bu, "max" },
        { 0xCBFD9u, "mgpubin" },
        { 0x10867u, "mid" },
        { 0xADB8Du, "msgbin" },
        { 0xE959Fu, "n3d2p_raw" },
        { 0xB9C3Cu, "nav" },
        { 0xDB3C3u, "nav_cell_ref" },
        { 0xC0A4Eu, "nav_connectivity" },
        { 0x9A9A0u, "nav_context" },
        { 0x6DB15u, "nav_debug_single" },
        { 0x6F5C9u, "nav_edgelinks" },
        { 0xE03AEu, "nav_ref" },
        { 0x921Bu, "nav_smp" },
        { 0xCD7CEu, "nav_waypoint" },
        { 0x2ECBFu, "nav_world" },
        { 0x4E016u, "nav_world_deps" },
        { 0x50B76u, "nav_world_map" },
        { 0x2C7CBu, "nav_world_splitter" },
        { 0x85D6Fu, "navalist" },
        { 0x216F8u, "navcelllist" },
        { 0x719C9u, "parambin" },
        { 0x7B665u, "pka" },
        { 0x7CC7Cu, "pkr" },
        { 0x49B7Cu, "pmdl" },
        { 0x61F66u, "png" },
        { 0x7C643u, "pngbin" },
        { 0x63A1Du, "prefab" },
        { 0x5AFu, "prt" },
        { 0x9D7F1u, "prtd" },
        { 0x9EC55u, "prtx" },
        { 0xE9D31u, "ps.sb" },
        { 0xB8247u, "psocache" },
        { 0xE101Cu, "r.btex" },
        { 0x4A6D3u, "rag" },
        { 0x6473Eu, "ragdoll" },
        { 0x928D1u, "raygmtl" },
        { 0x76A58u, "res_info" },
        { 0xDAB71u, "sapb" },
        { 0x31C1u, "sax" },
        { 0x4C153u, "sax(space)" },
        { 0x33E76u, "sb" },
        { 0x27896u, "sbd" },
        { 0xBEBC9u, "sbmdl" },
        { 0x736C7u, "ssd" },
        { 0xE1934u, "style" },
        { 0x325F9u, "swf" },
        { 0x8E661u, "swfb" },
        { 0x13D72u, "tcd" },
        { 0x14CBDu, "tcm" },
        { 0x14957u, "tco" },
        { 0xFD979u, "tcophysx" },
        { 0x53EF5u, "tga" },
        { 0x21976u, "tif" },
        { 0xAF7B8u, "tif_$h.btex" },
        { 0x5591Bu, "tif.btex" },
        { 0x78A01u, "tms" },
        { 0xE877Au, "tmsbin" },
        { 0x4E142u, "tnav" },
        { 0x1A21u, "tpd" },
        { 0xF5E1Au, "tpdbin" },
        { 0x7F1BBu, "tspack" },
        { 0x6AB79u, "txt" },
        { 0x1F9BCu, "uam" },
        { 0x1AF61u, "uifn" },
        { 0x89393u, "uip" },
        { 0xF6084u, "umbra" },
        { 0xD34BFu, "ups" },
        { 0xD2546u, "vegy" },
        { 0x91A20u, "vfol" },
        { 0x98B7Au, "vfuncs" },
        { 0x44689u, "vfx" },
        { 0xE23B5u, "vfxlist" },
        { 0x2761u, "vhlist" },
        { 0x81523u, "vlink" },
        { 0x3DFu, "vs.sb" },
        { 0xC7BC5u, "win.config" },
        { 0xCBF7Du, "win.mab" },
        { 0xDF19Fu, "win.sab" },
        { 0x6B501u, "win32.bin" },
        { 0x650B6u, "win32.bins" },
        { 0xA8B22u, "win32.msgbin" },
        { 0x58D48u, "wld_prc" },
        { 0x983EBu, "wlod" },
        { 0xA7AFFu, "wlodn" },
        { 0x883B8u, "wpcm" },
        { 0x899CFu, "wpcp" },
        { 0x1A5FCu, "wpcpbin" },
        { 0x658F5u, "wped" },
        { 0x7CDD6u, "wpvd" },
        { 0xF05A6u, "wth2" },
        { 0x6CC0Cu, "wth2b" },
        { 0x74EB8u, "xml" },
    };

    public const uint HEB256 = 0x1D37Du;
    public const uint HEB512 = 0x608D4u;
    public const uint HEB1024 = 0x66DBDu;
    public const uint HEB2048 = 0x193F0u;
    public const uint HEB4096 = 0xDE1ADu;
    public const uint CCB5 = 0xC1010u;
    public const uint COL2D = 0xA96B1u;
    public const uint ACD = 0xCBC81u;
    public const uint AIG = 0x99DCEu;
    public const uint AIIA = 0x676E7u;
    public const uint AIIA_ERUDITION_XML = 0xECC50u;
    public const uint AIIA_DBG = 0xC18DEu;
    public const uint AIIA_REF = 0xFD0B4u;
    public const uint ALIST = 0x7A4B2u;
    public const uint AMDL = 0xA5AB9u;
    public const uint ANI = 0x4F423u;
    public const uint ANMGPH = 0x56E70u;
    public const uint APB = 0xDD5A0u;
    public const uint APX = 0xE01CEu;
    public const uint ATLAS = 0xA68CAu;
    public const uint AUTOEXT = 0x5C91Bu;
    public const uint BCFG = 0xCC79Fu;
    public const uint BIN = 0xA31A0u;
    public const uint BINS = 0x2A989u;
    public const uint BLAST = 0x38FF3u;
    public const uint BLASTC = 0xCF1B0u;
    public const uint BLOS = 0x3B24Fu;
    public const uint BMDL = 0x474C2u;
    public const uint BNM = 0x28B3Eu;
    public const uint BNMWND = 0x8F081u;
    public const uint BOD = 0xEC2B0u;
    public const uint BTEX = 0x259C8u;
    public const uint BTEXHEADER = 0xA806Du;
    public const uint CCB = 0xEB241u;
    public const uint CCF = 0xEB90Du;
    public const uint CH_SB = 0x9ACD3u;
    public const uint CLSN = 0xA8C41u;
    public const uint CLSX = 0xA7477u;
    public const uint CMDL = 0x7B3BFu;
    public const uint COLOR_BTEX = 0xFA8B7u;
    public const uint COLOR_DDS_BCFG = 0xB925u;
    public const uint CONFIG = 0x32601u;
    public const uint CUSTOMDATA = 0x3D6D8u;
    public const uint DAT = 0x38AC8u;
    public const uint DATA = 0x49D2Bu;
    public const uint DB3 = 0xD1642u;
    public const uint DBG = 0xD6106u;
    public const uint DDS = 0xB1960u;
    public const uint DDS_BCFG = 0x80D26u;
    public const uint DEARC = 0x80CF0u;
    public const uint DML = 0xFD5BCu;
    public const uint DUMMY_TXT = 0x393A3u;
    public const uint DX11_FNTBIN = 0xE3850u;
    public const uint DYN = 0x9A5EEu;
    public const uint EARC = 0x2DCA6u;
    public const uint EARCREF = 0x3CF07u;
    public const uint EBEX = 0xDC0F5u;
    public const uint ELX = 0xFB046u;
    public const uint EMEM = 0x9276Fu;
    public const uint EMPATH = 0xF072Eu;
    public const uint ENTTRAY = 0xAA358u;
    public const uint EREP = 0x2E1FDu;
    public const uint EXML = 0xE5A3Fu;
    public const uint EXR = 0x5FB44u;
    public const uint FILE_LIST = 0x16D72u;
    public const uint FNTBIN = 0xE8C2Eu;
    public const uint FOLG = 0x190C9u;
    public const uint FOLGBIN = 0xB0B32u;
    public const uint GMDL = 0xEF4DBu;
    public const uint GMDL_HAIR = 0xD1078u;
    public const uint GMDL_GFXBIN = 0xA2343u;
    public const uint GMTL = 0x12F0Bu;
    public const uint GMTL_GFXBIN = 0xF2DB3u;
    public const uint GMTLA = 0x3911Eu;
    public const uint GPUBIN = 0xC5E90u;
    public const uint HEB = 0xDD95Au;
    public const uint HEPHYSX = 0x3108Cu;
    public const uint HIGH_FOLGBIN = 0x6E602u;
    public const uint HTPK = 0xC2CC2u;
    public const uint ID2S = 0x586A5u;
    public const uint IES = 0x70F36u;
    public const uint IKTS = 0xEC2F6u;
    public const uint IRR = 0xBC182u;
    public const uint JSON = 0x44D11u;
    public const uint KAB = 0x9473Bu;
    public const uint KDI = 0x196A7u;
    public const uint LAYER_INFO = 0x36D11u;
    public const uint LAYER_PCD = 0x64FA4u;
    public const uint LIK = 0xD9035u;
    public const uint LIPMAP = 0x677CAu;
    public const uint LIST = 0x571EBu;
    public const uint LISTB = 0xFEBCBu;
    public const uint LNKANI = 0x37D88u;
    public const uint LOW_TMSBIN = 0xEF836u;
    public const uint LSD = 0xB064Au;
    public const uint MASK_BTEX = 0xC830u;
    public const uint MASK_DDS_BCFG = 0x41EEu;
    public const uint MAX = 0x7A08Bu;
    public const uint MGPUBIN = 0xCBFD9u;
    public const uint MID = 0x10867u;
    public const uint MSGBIN = 0xADB8Du;
    public const uint N3D2P_RAW = 0xE959Fu;
    public const uint NAV = 0xB9C3Cu;
    public const uint NAV_CELL_REF = 0xDB3C3u;
    public const uint NAV_CONNECTIVITY = 0xC0A4Eu;
    public const uint NAV_CONTEXT = 0x9A9A0u;
    public const uint NAV_DEBUG_SINGLE = 0x6DB15u;
    public const uint NAV_EDGELINKS = 0x6F5C9u;
    public const uint NAV_REF = 0xE03AEu;
    public const uint NAV_SMP = 0x921Bu;
    public const uint NAV_WAYPOINT = 0xCD7CEu;
    public const uint NAV_WORLD = 0x2ECBFu;
    public const uint NAV_WORLD_DEPS = 0x4E016u;
    public const uint NAV_WORLD_MAP = 0x50B76u;
    public const uint NAV_WORLD_SPLITTER = 0x2C7CBu;
    public const uint NAVALIST = 0x85D6Fu;
    public const uint NAVCELLLIST = 0x216F8u;
    public const uint PARAMBIN = 0x719C9u;
    public const uint PKA = 0x7B665u;
    public const uint PKR = 0x7CC7Cu;
    public const uint PMDL = 0x49B7Cu;
    public const uint PNG = 0x61F66u;
    public const uint PNGBIN = 0x7C643u;
    public const uint PREFAB = 0x63A1Du;
    public const uint PRT = 0x5AFu;
    public const uint PRTD = 0x9D7F1u;
    public const uint PRTX = 0x9EC55u;
    public const uint PS_SB = 0xE9D31u;
    public const uint PSOCACHE = 0xB8247u;
    public const uint R_BTEX = 0xE101Cu;
    public const uint RAG = 0x4A6D3u;
    public const uint RAGDOLL = 0x6473Eu;
    public const uint RAYGMTL = 0x928D1u;
    public const uint RES_INFO = 0x76A58u;
    public const uint SAPB = 0xDAB71u;
    public const uint SAX = 0x31C1u;
    public const uint SAX_SPACE = 0x4C153u;
    public const uint SB = 0x33E76u;
    public const uint SBD = 0x27896u;
    public const uint SBMDL = 0xBEBC9u;
    public const uint SSD = 0x736C7u;
    public const uint STYLE = 0xE1934u;
    public const uint SWF = 0x325F9u;
    public const uint SWFB = 0x8E661u;
    public const uint TCD = 0x13D72u;
    public const uint TCM = 0x14CBDu;
    public const uint TCO = 0x14957u;
    public const uint TCOPHYSX = 0xFD979u;
    public const uint TGA = 0x53EF5u;
    public const uint TIF = 0x21976u;
    public const uint TIFH_BTEX = 0xAF7B8u;
    public const uint TIF_BTEX = 0x5591Bu;
    public const uint TMS = 0x78A01u;
    public const uint TMSBIN = 0xE877Au;
    public const uint TNAV = 0x4E142u;
    public const uint TPD = 0x1A21u;
    public const uint TPDBIN = 0xF5E1Au;
    public const uint TSPACK = 0x7F1BBu;
    public const uint TXT = 0x6AB79u;
    public const uint UAM = 0x1F9BCu;
    public const uint UIFN = 0x1AF61u;
    public const uint UIP = 0x89393u;
    public const uint UMBRA = 0xF6084u;
    public const uint UPS = 0xD34BFu;
    public const uint VEGY = 0xD2546u;
    public const uint VFOL = 0x91A20u;
    public const uint VFUNCS = 0x98B7Au;
    public const uint VFX = 0x44689u;
    public const uint VFXLIST = 0xE23B5u;
    public const uint VHLIST = 0x2761u;
    public const uint VLINK = 0x81523u;
    public const uint VS_SB = 0x3DFu;
    public const uint WIN_CONFIG = 0xC7BC5u;
    public const uint WIN_MAB = 0xCBF7Du;
    public const uint WIN_SAB = 0xDF19Fu;
    public const uint WIN32_BIN = 0x6B501u;
    public const uint WIN32_BINS = 0x650B6u;
    public const uint WIN32_MSGBIN = 0xA8B22u;
    public const uint WLD_PRC = 0x58D48u;
    public const uint WLOD = 0x983EBu;
    public const uint WLODN = 0xA7AFFu;
    public const uint WPCM = 0x883B8u;
    public const uint WPCP = 0x899CFu;
    public const uint WPCPBIN = 0x1A5FCu;
    public const uint WPED = 0x658F5u;
    public const uint WPVD = 0x7CDD6u;
    public const uint WTH2 = 0xF05A6u;
    public const uint WTH2B = 0x6CC0Cu;
    public const uint XML = 0x74EB8u;
}

namespace Scarlet.Structures;

public static class TypeIdRegistry {
    public static Dictionary<uint, string> IdTable { get; set; } = new() {
        { 0x66DBDu, "1024.heb" },
        { 0x193F0u, "2048.heb" },
        { 0x1D37Du, "256.heb" },
        { 0xDE1ADu, "4096.heb" },
        { 0x608D4u, "512.heb" },
        { 0xCBC81u, "acd" },
        { 0x99DCEu, "aig" },
        { 0x7A4B2u, "alist" },
        { 0xA5AB9u, "amdl" },
        { 0x4F423u, "ani" },
        { 0x56E70u, "anmgph" },
        { 0xDD5A0u, "apb" },
        { 0xA68CAu, "atlas" },
        { 0xA31A0u, "bin" },
        { 0x38FF3u, "blast" },
        { 0xCF1B0u, "blastc" },
        { 0x28B3Eu, "bnm" },
        { 0x259C8u, "btex" },
        { 0xEB241u, "ccb" },
        { 0xEB90Du, "ccf" },
        { 0x9ACD3u, "ch.sb" },
        { 0xA8C41u, "clsn" },
        { 0x7B3BFu, "cmdl" },
        { 0x32601u, "config" },
        { 0x3D6D8u, "customdata" },
        { 0x49D2Bu, "data" },
        { 0xB1960u, "dds" },
        { 0x393A3u, "dummy.txt" },
        { 0x2DCA6u, "earc" },
        { 0xDC0F5u, "ebex" },
        { 0xFB046u, "elx" },
        { 0xF072Eu, "empath" },
        { 0x2E1FDu, "erep" },
        { 0xE5A3Fu, "exml" },
        { 0x5FB44u, "exr" },
        { 0x16D72u, "file_list" },
        { 0x190C9u, "folg" },
        { 0xB0B32u, "folgbin" },
        { 0xEF4DBu, "gmdl" },
        { 0xD1078u, "gmdl_hair" },
        { 0xA2343u, "gmdl.gfxbin" },
        { 0x12F0Bu, "gmtl" },
        { 0xF2DB3u, "gmtl.gfxbin" },
        { 0xC5E90u, "gpubin" },
        { 0xDD95Au, "heb" },
        { 0x3108Cu, "hephysx" },
        { 0xC2CC2u, "htpk" },
        { 0x586A5u, "id2s" },
        { 0x44D11u, "json" },
        { 0x196A7u, "kdi" },
        { 0x36D11u, "layer_info" },
        { 0x64FA4u, "layer_pcd" },
        { 0x677CAu, "lipmap" },
        { 0x571EBu, "list" },
        { 0xB064Au, "lsd" },
        { 0xE959Fu, "n3d2p_raw" },
        { 0xB9C3Cu, "nav" },
        { 0xDB3C3u, "nav_cell_ref" },
        { 0xC0A4Eu, "nav_connectivity" },
        { 0x6DB15u, "nav_debug_single" },
        { 0x6F5C9u, "nav_edgelinks" },
        { 0x921Bu, "nav_smp" },
        { 0x4E016u, "nav_world_deps" },
        { 0x50B76u, "nav_world_map" },
        { 0x85D6Fu, "navalist" },
        { 0x216F8u, "navcelllist" },
        { 0x719C9u, "parambin" },
        { 0x7B665u, "pka" },
        { 0x7CC7Cu, "pkr" },
        { 0x49B7Cu, "pmdl" },
        { 0x61F66u, "png" },
        { 0xE9D31u, "ps.sb" },
        { 0xB8247u, "psocache" },
        { 0xE101Cu, "r.btex" },
        { 0x6473Eu, "ragdoll" },
        { 0x76A58u, "res_info" },
        { 0xDAB71u, "sapb" },
        { 0x31C1u, "sax" },
        { 0x33E76u, "sb" },
        { 0x27896u, "sbd" },
        { 0x53EF5u, "tga" },
        { 0x21976u, "tif" },
        { 0x1A21u, "tpd" },
        { 0xF5E1Au, "tpdbin" },
        { 0x7F1BBu, "tspack" },
        { 0x6AB79u, "txt" },
        { 0x1AF61u, "uifn" },
        { 0x89393u, "uip" },
        { 0xD2546u, "vegy" },
        { 0x44689u, "vfx" },
        { 0xE23B5u, "vfxlist" },
        { 0x3DFu, "vs.sb" },
        { 0xC7BC5u, "win.config" },
        { 0xDF19Fu, "win.sab" },
        { 0x58D48u, "wld_prc" },
        { 0x983EBu, "wlod" },
        { 0xA7AFFu, "wlodn" },
        { 0x883B8u, "wpcm" },
        { 0x899CFu, "wpcp" },
        { 0x1A5FCu, "wpcpbin" },
        { 0x658F5u, "wped" },
        { 0x7CDD6u, "wpvd" },
    };

    public const uint HEB256 = 0x1D37Du;
    public const uint HEB512 = 0x608D4u;
    public const uint HEB1024 = 0x66DBDu;
    public const uint HEB2048 = 0x193F0u;
    public const uint HEB4096 = 0xDE1ADu;
    public const uint ACD = 0xCBC81u;
    public const uint AIG = 0x99DCEu;
    public const uint ALIST = 0x7A4B2u;
    public const uint AMDL = 0xA5AB9u;
    public const uint ANI = 0x4F423u;
    public const uint ANMGPH = 0x56E70u;
    public const uint APB = 0xDD5A0u;
    public const uint ATLAS = 0xA68CAu;
    public const uint BIN = 0xA31A0u;
    public const uint BLAST = 0x38FF3u;
    public const uint BLASTC = 0xCF1B0u;
    public const uint BNM = 0x28B3Eu;
    public const uint BTEX = 0x259C8u;
    public const uint CCB = 0xEB241u;
    public const uint CCF = 0xEB90Du;
    public const uint CH_SB = 0x9ACD3u;
    public const uint CLSN = 0xA8C41u;
    public const uint CMDL = 0x7B3BFu;
    public const uint CONFIG = 0x32601u;
    public const uint CUSTOMDATA = 0x3D6D8u;
    public const uint DATA = 0x49D2Bu;
    public const uint DDS = 0xB1960u;
    public const uint DUMMY_TXT = 0x393A3u;
    public const uint EARC = 0x2DCA6u;
    public const uint EBEX = 0xDC0F5u;
    public const uint ELX = 0xFB046u;
    public const uint EMPATH = 0xF072Eu;
    public const uint EREP = 0x2E1FDu;
    public const uint EXML = 0xE5A3Fu;
    public const uint EXR = 0x5FB44u;
    public const uint FILE_LIST = 0x16D72u;
    public const uint FOLG = 0x190C9u;
    public const uint FOLGBIN = 0xB0B32u;
    public const uint GMDL = 0xEF4DBu;
    public const uint GMDL_HAIR = 0xD1078u;
    public const uint GMDL_GFXBIN = 0xA2343u;
    public const uint GMTL = 0x12F0Bu;
    public const uint GMTL_GFXBIN = 0xF2DB3u;
    public const uint GPUBIN = 0xC5E90u;
    public const uint HEB = 0xDD95Au;
    public const uint HEPHYSX = 0x3108Cu;
    public const uint HTPK = 0xC2CC2u;
    public const uint ID2S = 0x586A5u;
    public const uint JSON = 0x44D11u;
    public const uint KDI = 0x196A7u;
    public const uint LAYER_INFO = 0x36D11u;
    public const uint LAYER_PCD = 0x64FA4u;
    public const uint LIPMAP = 0x677CAu;
    public const uint LIST = 0x571EBu;
    public const uint LSD = 0xB064Au;
    public const uint N3D2P_RAW = 0xE959Fu;
    public const uint NAV = 0xB9C3Cu;
    public const uint NAV_CELL_REF = 0xDB3C3u;
    public const uint NAV_CONNECTIVITY = 0xC0A4Eu;
    public const uint NAV_DEBUG_SINGLE = 0x6DB15u;
    public const uint NAV_EDGELINKS = 0x6F5C9u;
    public const uint NAV_SMP = 0x921Bu;
    public const uint NAV_WORLD_DEPS = 0x4E016u;
    public const uint NAV_WORLD_MAP = 0x50B76u;
    public const uint NAVALIST = 0x85D6Fu;
    public const uint NAVCELLLIST = 0x216F8u;
    public const uint PARAMBIN = 0x719C9u;
    public const uint PKA = 0x7B665u;
    public const uint PKR = 0x7CC7Cu;
    public const uint PMDL = 0x49B7Cu;
    public const uint PNG = 0x61F66u;
    public const uint PS_SB = 0xE9D31u;
    public const uint PSOCACHE = 0xB8247u;
    public const uint R_BTEX = 0xE101Cu;
    public const uint RAGDOLL = 0x6473Eu;
    public const uint RES_INFO = 0x76A58u;
    public const uint SAPB = 0xDAB71u;
    public const uint SAX = 0x31C1u;
    public const uint SB = 0x33E76u;
    public const uint SBD = 0x27896u;
    public const uint TGA = 0x53EF5u;
    public const uint TIF = 0x21976u;
    public const uint TPD = 0x1A21u;
    public const uint TPDBIN = 0xF5E1Au;
    public const uint TSPACK = 0x7F1BBu;
    public const uint TXT = 0x6AB79u;
    public const uint UIFN = 0x1AF61u;
    public const uint UIP = 0x89393u;
    public const uint VEGY = 0xD2546u;
    public const uint VFX = 0x44689u;
    public const uint VFXLIST = 0xE23B5u;
    public const uint VS_SB = 0x3DFu;
    public const uint WIN_CONFIG = 0xC7BC5u;
    public const uint WIN_SAB = 0xDF19Fu;
    public const uint WLD_PRC = 0x58D48u;
    public const uint WLOD = 0x983EBu;
    public const uint WLODN = 0xA7AFFu;
    public const uint WPCM = 0x883B8u;
    public const uint WPCP = 0x899CFu;
    public const uint WPCPBIN = 0x1A5FCu;
    public const uint WPED = 0x658F5u;
    public const uint WPVD = 0x7CDD6u;
}

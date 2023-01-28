enum EARC_ARCHIVE_FLAGS : u8 {
    Encrypted = 0x80,
};

enum EARC_HEADER_FLAGS : u32 {
    Loose = 1,
    Localized = 2,
    Debug = 4,
    AdvanceChecksum = 8,
};

enum EARC_FILE_FLAGS : u32 {
    AutoLoad = 1,
    Compressed = 2,
    Reference = 4,
    Loose = 8,
    Patched = 0x10,
    Deleted = 0x20,
    Encrypted = 0x40,
    Protected = 0x80,
    HasCompressionType = 0x10000000, // this is an assumption, Forspoken has 1 + 4 always set when compressed.
    CompressionTypeZlib = 0x20000000
    CompressionTypeLZ4 = 0x40000000
};

struct earc_header {
    u32 magic;
    u8 version_major;
    u8 version_minor;
    u8 version_patch;
    EARC_ARCHIVE_FLAGS archive_flags;
    u32 file_count;
    u32 block_size;
    u32 file_allocation_table_offset;
    u32 file_name_table_offset;
    u32 archive_path_table_offset;
    u32 file_data_offset;
    EARC_HEADER_FLAGS flags;
    u32 chunk_size;
    u64 checksum;
};

struct earc_file_entry {
    u64 checksum;
    u32 size;
    u32 czsize;
    EARC_FILE_FLAGS flags;
    u32 name_offset;
    u64 data_offset;
    u32 archive_offset;
    u8 type;
    u8 locale;
    u16 key;
};

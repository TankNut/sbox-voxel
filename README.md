# sbox-voxel
A set of library classes for s&amp;box to create runtime voxel models

## Installing
Merge the files with your gamemode however you want, for developing I have the two kv6 folders symlinked into my local copy of [facepunch/sandbox](https://github.com/Facepunch/sandbox)

## Usage
```cs
VoxelBuilder builder = new();

builder.Set( 0, 0, 0, new Color32( 255, 0, 0 ) );
builder.Set( 1, 0, 0, new Color32( 255, 0, 0 ) );
builder.Set( 0, 1, 0, new Color32( 0, 255, 0 ) );
builder.Set( 1, 1, 0, new Color32( 0, 255, 0 ) );

builder.Set( 0, 0, 10, new Color32( 0, 0, 255 ) );
builder.Set( 1, 0, 10, new Color32( 0, 0, 255 ) );
builder.Set( 0, 1, 10, new Color32( 0, 0, 255 ) );
builder.Set( 1, 1, 10, new Color32( 0, 0, 255 ) );

builder.Build( "test", 8.0f );
```

This will result in the following prop when spawned with `spawn_kv6`

![example.png](https://github.com/TankNut/sbox-voxel/blob/master/example.png?raw=true)

Check out [Kv6Loader.cs](Kv6Loader.cs) for more examples

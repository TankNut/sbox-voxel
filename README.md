# sbox-voxel
A set of library classes for s&amp;box to create runtime voxel models

## Installing
Merge the files with your gamemode however you want, for developing I have the two voxel folders symlinked into my local copy of [facepunch/sandbox](https://github.com/Facepunch/sandbox)

## Supported formats

|Format|File Signature|Extension|Notes|
|---|---|---|---|
|Voxlap|"Kvxl"|.kv6||
|MagicaVoxel|"VOX "|.vox|Only RGBA and XYZI chunks are handled, multiple XYZI chunks should work but haven't been tested|

## Usage
Models can be manually built and registered through the following method:

```cs
VoxelBuilder builder = new();

builder.Set( 0, 0, 0, new Color( 1, 0, 0 ) );
builder.Set( 1, 0, 0, new Color( 1, 0, 0 ) );
builder.Set( 0, 1, 0, new Color( 0, 1, 0 ) );
builder.Set( 1, 1, 0, new Color( 0, 1, 0 ) );

builder.Set( 0, 0, 10, new Color( 0, 0, 1 ) );
builder.Set( 1, 0, 10, new Color( 0, 0, 1 ) );
builder.Set( 0, 1, 10, new Color( 0, 0, 1 ) );
builder.Set( 1, 1, 10, new Color( 0, 0, 1 ) );

VoxelManager.RegisterModel("test", builder.Build());
```

This will result in the following prop when spawned with `spawn_voxel test 8`

![example.png](https://github.com/TankNut/sbox-voxel/blob/master/example.png?raw=true)

If you want to load external files you can use one of the existing importers or create your own by implementing `IVoxelLoader` into your class and registering it through `VoxelManager.RegisterLoader(string signature, IVoxelLoader loader)`, after that you'll be able to load your models through any of the `VoxelManager.LoadFrom*` functions

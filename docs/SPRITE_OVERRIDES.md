# External sprite overrides

Freeserf can optionally load replacement PNG graphics without modifying the
original Ubisoft data files.

## Location

By default, place files under:

`<application-local-data>/Freeserf/graphics/`

The root can be changed with the `FREESERF_GRAPHICS_DIR` environment variable.

## File naming

The directory name is the lower-case `Data.Resource` name and the filename is
the numeric sprite index padded to three digits:

```
graphics/
  map_object/
    000.png
    001.png
    ...
    127.png
    144.png
    ...
    193.png
```

The current integration checks world map objects (0–127) and building sprites
(144–193). Other resource categories are not overridden yet.

## Important compatibility rules

* PNG must have an alpha channel (transparent background).
* Width and height must exactly match the original sprite.
* Keep the original sprite canvas and baseline. The engine continues to use
  the original DataSource sprite metadata for offsets and placement.
* Use the game's existing player-color mask system for flags and player-colored
  serf clothing; these are not replaced by this first override pass.
* Missing, malformed, or wrong-sized files are ignored safely. The original
  game sprite is used instead.
* Do not commit Ubisoft game data or extracted Ubisoft sprites to this repo.
  Only original artwork that you have rights to distribute should be added.

To determine the required dimensions for an index, inspect the original
resource with `DataSource.GetSpriteInfo(resource, index)` while the game data
is loaded.

## Distance Objects
Certain [configuration file](config.md) values need to be convertible
to `Distance` objects.

The general format for such a text string is this:
```
<number> <type of distance>
```
where **<number\>** can be an integer or decimal while **<distance\>**
must be one of the `UnitTypes`:
- ft
- mi
- m
- km

The space between the number and unit type is significant and must
be included.

Negative or zero values will be replaced by default values. The specific
default value depends on the property.
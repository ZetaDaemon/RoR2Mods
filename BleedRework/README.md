# Bleed Rework
Reworks bleed to be based on the damage dealt, rather than a fixed damage value. Dealing damage 3 times a second over 3 seconds. Bleed is guarenteed to be applied, no longer effected by proc coefficient, and the duration no longer resets on application.

## Tritip Dagger
Applies 20% of total damage as bleed.

## Noxious Thorn
Same as vanilla, effectively granting a single tritip, but updated with the new tritip mechanics. As a note, the internal functionality for debuff spreading had to be reimplemented as normally the damage of a dot isn't accounted for, this may lead to issues with other mods that influence thorn.

## Shatterspleen
Applies 40% of critical hit damage as bleed. Bleeding enemies explode for 100% (per stack) of the remaining bleed dot damage.

## Hemmorage
Hemmorage now ticks 5 times a second and lasts for 6 seconds. Serrated Dagger and Shiv hemmorage enemies for 600% damage dealt.
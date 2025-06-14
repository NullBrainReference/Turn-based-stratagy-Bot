It was created on top of existing game, which lacked models of units, items, tiles, items were like if-else cases and so on.

Bot (Move based) was kind of rushed solution, so in many cases i had not a lot of option for refactor anything. You can find tonn of magic moveWeight values, switch cases for items etc.
For Bot2 i had a slower approach it was planned during lates Bot1 adjustments. But there was a problem Players have to push 3 Actions, and after the first move it's a game with unknown data, so 
1) You cannot cut any pos brunches, (even dumbest may be a trickery).
2) Bot have to play for min loss, it's only advantage is first move tempo.
3) There is a need for grouping moves - all responses, and planned action may not meet real one due to death or pos change by player response.
4) There are a tonn of wariants so fiters and filter lines were made, to sease large variants of 5M-15M (theoreticly there may be 100M+) to 1.5K-50K multiple lines calced async as Task


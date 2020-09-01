# dallas-precinct-finder
Finds the precinct number of a Dallas County address.

Place a .txt file with addresses in same directory as this executable.
Create a file with a zipcode, followed by each address on its own line. Repeat for each zipcode.
For best results, remove everything after the street name.

Example:
```sh
75041
1001 Riverchase
1003 Riverchase
75048
2001 Main
2003 Main
```

Will output a file with same name + _PRECINCTS.txt

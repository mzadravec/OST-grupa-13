OST-grupa-13
============
Aplikacija povezuje GAPED bazu sa sinsetima iz WordNet rjecnika. Kroz aplikaciju je moguce pregledavati slike iz GAPED baze, dodavati im sinsete iz WordNet-a te pregledavati dodane sinsete. Sve se trajno sprema u SQLite bazu.
SQLite baza, GAPED baza i WordNet rjecnik dolaze skupa s aplikacijom te nije potrebno nista dodatno instalirati.

Baza je sqlite3 baza, tablica slika se naziva Picture i ima dva atributa, jedinstveni id slike i relativnu putanju do slike koje se sve nalaze u GAPED/ direktoriju.
Skripta picturePaths.sh popunjava bazu relativnim putanjama do slika ukoliko je potrebno dodati jos slika.

Najbitniji dokumenti su  MainWindow.xaml (izgled), MainWindow.xaml.cs(akcije) i Picture.cs(model slike).

Za pristup i koristenje WordNet-a koristen je WordNetAPI https://github.com/zacg/WordNetAPI od Matt Gerbera.
Stanje repozitorija WordNetAPI-a kakav je bio 16.1.2014. spremljeno je u repozitoriju.

Upozorenje: ukoliko se nakon pullanja repozitorija pojavljuje greska u aplikaciji pri pretrazivanju WordNet-a, potrebno je sadrzaj direktorija dict zamijeniti sa sadrzajem direktorija dict_init.
Problem se dogadja jer WordNetApi sortira WordNet rjecnik (koji se nalazi u dict) na nacin koji njemu odgovara, te se pri prijenosu takvog sortiranog rjecnika sa jednog racunalo na drugo dolazi nekad do problema.
Direktorij dict_init sadrzi izvorni rjecnik, prije sortiranja, pa kopiranjem sadrzaja iz njega u dict tjeramo WordNetApi da ponovno provede sortiranje te onda sve radi.
Ako problem i dalje nije rjesen, skinuti izvorni rjecnik sa linka: http://wordnetcode.princeton.edu/3.0/WNdb-3.0.tar.gz i zamijeniti sadrzaj dict direktorija s njim.
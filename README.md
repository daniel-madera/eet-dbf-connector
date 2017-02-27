# EET-DBF-CONNECTOR
Konektor pro zasl�n� EET a z�sk�n� FIK na z�klad� hodnot z DBF souboru.

## Knihovna EET-LITE
https://github.com/l-ra/openeet-net.git

## Zm�na konfigurace
* Nastavit spr�vn� hodnoty v souboru eet_conv.exe.config.
* Produk�n� URL pro EET po�adavek je https://prod.eet.cz:443/eet/services/EETServiceSOAP/v3.
* Testovac� URL pro EET po�adavek je https://pg.eet.cz:443/eet/services/EETServiceSOAP/v3.

Pozn. Pozor na n�zev souboru DBF. Cesta nesm� obsahovat speci�ln� znaky jako pom�ka, mezera apod.

## Spo�t�n� procedury v PCFAND
```pascal
exec('CMD','/c call SET WINDIR=C:\WINDOWS& cd C:\Sklad_z\SkladEet\Eet& eet_conv.exe',nocancel);
case Eet.fik=~'': begin; 
	{P��klad FIK nebylo p�ed�no.}
end;
```

## Vygenerov�n� za�ifrovan�ho hesla
```bash
$>cd C:\EetConvDir\
$>eet_conv.exe encrypt_password heslohovnokleslo
Nov� heslo: /6Bj7e4BsxdXT35lH7iBi0g9uzSbeT67gqxhSx15yTs=
P�epi�te hodnotu polo�ky CRT_PASS v souboru eet_conv.exe.config na nov� heslo a spus�te program znovu bez parametr�.
```
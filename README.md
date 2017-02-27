# EET-DBF-CONNECTOR
Konektor pro zaslání EET a získání FIK na základì hodnot z DBF souboru.

## Knihovna EET-LITE
https://github.com/l-ra/openeet-net.git

## Zmìna konfigurace
* Nastavit správné hodnoty v souboru eet_conv.exe.config.
* Produkèní URL pro EET poadavek je https://prod.eet.cz:443/eet/services/EETServiceSOAP/v3.
* Testovací URL pro EET poadavek je https://pg.eet.cz:443/eet/services/EETServiceSOAP/v3.

Pozn. Pozor na název souboru DBF. Cesta nesmí obsahovat speciální znaky jako pomèka, mezera apod.

## Spoštìní procedury v PCFAND
```pascal
exec('CMD','/c call SET WINDIR=C:\WINDOWS& cd C:\Sklad_z\SkladEet\Eet& eet_conv.exe',nocancel);
case Eet.fik=~'': begin; 
	{Pøíklad FIK nebylo pøedáno.}
end;
```

## Vygenerování zašifrovaného hesla
```bash
$>cd C:\EetConvDir\
$>eet_conv.exe encrypt_password heslohovnokleslo
Nové heslo: /6Bj7e4BsxdXT35lH7iBi0g9uzSbeT67gqxhSx15yTs=
Pøepište hodnotu poloky CRT_PASS v souboru eet_conv.exe.config na nové heslo a spuste program znovu bez parametrù.
```
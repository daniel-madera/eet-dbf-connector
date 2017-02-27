# EET-DBF-CONNECTOR
Konektor pro zaslání EET a získání FIK na základì hodnot z DBF souboru.

## Zmìna konfigurace
* Nastavit správné hodnoty v souboru eet_conv.exe.config.
* Produkèní URL pro EET poadavek je https://prod.eet.cz:443/eet/services/EETServiceSOAP/v3.

Pozn. Pozor na název souboru DBF. Cesta nesmí obsahovat speciální znaky jako pomèka, mezera apod.

## Vygenerování zašifrovaného hesla
```bash
C:\Users\User>cd C:\EetConvDir\
C:\EetConvDir\>eet_conv.exe encrypt_password heslohovnokleslo
Nové heslo: /6Bj7e4BsxdXT35lH7iBi0g9uzSbeT67gqxhSx15yTs=
Pøepište hodnotu poloky CRT_PASS v souboru eet_conv.exe.config na nové heslo a spuste program znovu bez parametrù.
```
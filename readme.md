# Sem5_JiPP_Micro 🎮

Projekt rozproszonej gry zręcznościowej opartej na platformie Micro:bit z centralnym systemem zapisu i prezentacji wyników.

## Architektura sprzętowa i przepływ danych

Logika samej gry jest w całości zaimplementowana na mikrokontrolerze **Micro:bit**. Ze względu na brak wbudowanego modułu Wi-Fi w klasycznym Micro:bicie, komunikacja ze światem zewnętrznym odbywa się za pośrednictwem dodatkowego mikrokontrolera.

* **Micro:bit** jest połączony kablem z modułem **Arduino**. 
* Komunikacja między nimi odbywa się przez interfejs szeregowy (**UART**). 
* Gdy gracz kończy grę, Micro:bit wysyła prostą ramkę z ID gracza i wynikiem na port szeregowy. 
* **Arduino** pełni tu wyłącznie rolę bramy sieciowej (gateway) – odbiera dane z UART, pakuje je w format JSON i wysyła przez Wi-Fi do serwera.

## Backend i Baza Danych

Po stronie serwera aplikacja jest napisana w **C#** z wykorzystaniem frameworka **ASP.NET Core**. API wystawia proste endpointy RESTowe:

* `POST /api/scores` – do przyjmowania nowych wyników z Arduino.
* `GET /api/scores/top` – do pobierania aktualnego rankingu.

Jako bazę danych wykorzystamy **Microsoft SQL Server**. Baza posiada prostą strukturę, np. tabelę `Leaderboard` z kolumnami: *ID, Nazwa Użytkownika, Wynik oraz Data uzyskania wyniku*. Do komunikacji API z bazą i mapowania obiektowo-relacyjnego użyty jest **Entity Framework Core**, co mocno przyspiesza pisanie zapytań i zapewnia bezpieczeństwo (ochrona przed SQL Injection).

## Prezentacja rankingu

Ranking jest prezentowany na dwa sposoby:

1. **Webowo:** Lekka aplikacja frontendowa wyświetlająca posortowaną tabelę wyników.
2. **Fizycznie (Hardware):** Drugie Arduino cyklicznie odpytuje serwer, wykonując zapytanie `GET` na endpoint. Żeby nie obciążać pamięci mikrokontrolera parsowaniem dużych plików JSON, API może mieć specjalny endpoint zwracający dane w czystym tekście oddzielonym przecinkami (CSV), które Arduino łatwo przetworzy i rzuci na ekranik.

## Środowisko uruchomieniowe i Wirtualizacja

Aby ułatwić wdrożenie i odizolować aplikację, backend oraz baza MS SQL Server będą skonteneryzowane przy użyciu **Dockera**. Konfiguracja zostanie opisana w pliku `docker-compose.yml`, co pozwoli na postawienie całego środowiska jedną komendą na dowolnym serwerze.

Całość kontenerów docelowo zostanie wdrożona na prostym serwerze VPS lub maszynie wirtualnej w chmurze (np. Azure / AWS). Arduino będzie uderzać bezpośrednio na publiczny adres IP/domenę serwera. Żeby uniknąć spamu wysyłanego z zewnątrz, endpoint `POST` w API będzie wymagał prostego tokena autoryzacyjnego w nagłówku HTTP, zaszytego na stałe w kodzie Arduino.

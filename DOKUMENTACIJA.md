# Kratka dokumentacija projekta

## Tema projekta

Projekat predstavlja sistem za pracenje akvaticnih vrsta u akvarijumu. Sistem je realizovan kroz dve povezane WPF komponente koje komuniciraju preko WCF/CoreWCF servisa.

- **Komponenta 1 - Informacioni sistem**: vodi evidenciju o vrstama (`AquaticSpecies`) i merenjima kvaliteta vode (`WaterQualityReading`), omogucava CRUD operacije, pretragu, validaciju, undo/redo, logovanje aktivnosti, simulaciju novih merenja, graficki prikaz stanja i cuvanje/ucitavanje podataka.
- **Komponenta 2 - Statisticka obrada podataka**: preko WCF servisa preuzima podatke iz Komponente 1, prilagodjava ih trazenoj strukturi, prikazuje ih korisniku, izvrsava izabranu statisticku metodu i izvozi rezultate u CSV datoteku.
- **Shared**: sadrzi zajednicke modele i WCF ugovore koje koriste obe komponente.

## Model podataka

Glavni entitet je `AquaticSpecies`, koji predstavlja akvaticnu vrstu sa identifikatorom, nazivom, naucnim nazivom, stanistem, tipom vode i prosecnim zivotnim vekom.

Glavna metrika je `WaterQualityReading`, koja predstavlja merenje kvaliteta vode za odredjenu vrstu. Merenje sadrzi vreme merenja, pH vrednost, temperaturu, nivo kiseonika i stanje kvaliteta vode. Stanje je definisano enumeracijom `WaterQualityState` sa vrednostima `Optimal`, `Acceptable`, `Suboptimal` i `Critical`.

## Arhitektura

Obe komponente su organizovane po MVVM principu:

- `Views` sadrze XAML prikaz i minimalnu logiku vezanu za korisnicki interfejs.
- `ViewModels` sadrze stanje ekrana, komande i tok rada aplikacije.
- `Models` i `Shared.Models` sadrze podatke.
- `Services` sadrze poslovnu logiku, pristup podacima, komunikaciju, eksport, validaciju i pomocne servise.
- `Interfaces` definisu ugovore izmedju slojeva.

Komponenta 1 hostuje WCF servis kroz `CoreWcfHostService`, a implementacija ugovora je `AquariumService`. Komponenta 2 pravi WCF proxy prema interfejsu `IAquariumService`, a zatim koristi `AquariumClient` kao klijentski omotac za pozive servisa.

## Tok komunikacije

1. Korisnik u Komponenti 2 bira vrstu i mesec.
2. `StatisticsViewModel` poziva `IAquariumClient`.
3. `AquariumClient` salje zahtev WCF servisu preko `IAquariumService`.
4. `AquariumService` u Komponenti 1 validira zahtev i filtrira merenja iz repozitorijuma.
5. Komponenta 2 dobija listu merenja i prosledjuje je `WaterQualityAdapter` klasi.
6. Adapter mapira listu u recnik gde je kljuc oblika `SpeciesId-Name`, a vrednost lista merenja.
7. Korisnik bira statisticku metodu, a `StatisticsService` izvrsava odgovarajucu strategiju.

## Korisceni design patterni

| Pattern | Gde se koristi | Svrha |
| --- | --- | --- |
| **MVVM (Model-View-ViewModel)** | `MainWindow.xaml`, `MainWindowViewModel`, `StatisticsView.xaml`, `StatisticsViewModel`, `ObservableObject` | Razdvaja korisnicki interfejs od stanja i poslovne logike. ViewModel izlozi podatke i komande, dok View samo prikazuje i vezuje podatke. |
| **Command** | `IApplicationCommand`, `AddSpeciesCommand`, `UpdateSpeciesCommand`, `DeleteSpeciesCommand`, `AddReadingCommand`, `UpdateReadingCommand`, `DeleteReadingCommand`, `CommandExecutor` | Svaka izmena podataka je enkapsulirana kao objekat sa `Execute` i `Undo` metodama. Time je omogucen undo/redo mehanizam. |
| **Strategy** | `IStatisticsStrategy`, `AveragePhStrategy`, `MinimalOxygenStrategy`, `CriticalCountStrategy` | Omogucava zamenu algoritma statisticke obrade bez izmene ViewModel-a ili servisa koji ga koristi. Svaka statisticka metoda je posebna strategija. |
| **Factory / Simple Factory** | `IStatisticsStrategyFactory`, `StatisticsStrategyFactory` | Na osnovu naziva metode bira odgovarajucu statisticku strategiju. Time se logika izbora algoritma izdvaja iz `StatisticsService`. |
| **Adapter** | `WaterQualityAdapter` | Prilagodjava listu `WaterQualityReading` objekata strukturi trazenoj specifikacijom Komponente 2: `Dictionary<string, List<WaterQualityReading>>`. |
| **Repository** | `IAquaticSpeciesRepository`, `AquaticSpeciesRepository`, `IWaterQualityReadingRepository`, `WaterQualityReadingRepository` | Izoluje pristup kolekcijama podataka i centralizuje operacije citanja, dodavanja, izmene, brisanja i zamene podataka. |


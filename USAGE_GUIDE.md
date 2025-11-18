# Instrukcja Użytkowania - Algorytmy Binaryzacji

## Szybki Start

1. **Uruchomienie aplikacji:**
   ```bash
   dotnet run --project src/BiometricsApp.UI/BiometricsApp.UI.csproj
   ```

2. **Załadowanie obrazu:**
   - Kliknij przycisk "Open" w pasku narzędzi
   - Wybierz obraz (PNG, JPG, JPEG, BMP, GIF)

3. **Wybór algorytmu:**
   - Z listy "Select Operation" wybierz jeden z algorytmów binaryzacji

4. **Dostosowanie parametrów:**
   - Każdy algorytm ma swój panel parametrów
   - Użyj suwaków do dostosowania wartości

5. **Zastosowanie operacji:**
   - Kliknij "Apply Operation"
   - Poczekaj na przetworzenie
   - Zobacz wynik w dolnym panelu

6. **Cofnięcie operacji:**
   - Kliknij "Undo" aby cofnąć ostatnią operację
   - Możesz cofnąć do 20 ostatnich operacji

7. **Zapis wyniku:**
   - Kliknij "Save" aby zapisać przetworzony obraz
   - Wybierz format (PNG lub JPEG)

## Dostępne Algorytmy Binaryzacji

### 1. Threshold Binarization
Prosta binaryzacja z progiem.
- **Parametry:** Threshold (0-255), Channel

### 2. Otsu Binarization
Automatyczny wybór progu metodą Otsu.
- **Parametry:** Channel

### 3. Niblack Binarization ⭐
Lokalna adaptacyjna binaryzacja.
- **Parametry:** 
  - K Parameter (-1.0 do 0.5) - kontroluje czułość
  - Window Size (3-51) - rozmiar okna lokalnego

### 4. Sauvola Binarization ⭐
Ulepszona wersja Niblacka dla dokumentów.
- **Parametry:**
  - K Parameter (0.0-1.0)
  - Window Size (3-51)

### 5. Phansalkar Binarization ⭐
Dla obrazów o niskim kontraście.
- **Parametry:**
  - K Parameter (0.0-1.0)
  - Window Size (3-51)

### 6. Kapur Binarization
Binaryzacja oparta na entropii.
- **Parametry:** Brak (automatyczny)
- **Wynik:** Wyświetla obliczony próg

### 7. Li-Wu Binarization
Minimalna entropia krzyżowa.
- **Parametry:** Brak (automatyczny)
- **Wynik:** Wyświetla obliczony próg

### 8. Bernsen Binarization
Lokalna binaryzacja oparta na kontraście.
- **Parametry:**
  - Window Size (3-101)
  - Contrast Threshold (0-100)

### 9. Adaptive Gradient Binarization ✨🎯
**WŁASNY ALGORYTM** - łączy detekcję krawędzi z adaptacyjnym progowaniem.
- **Parametry:**
  - Window Size (3-51)
  - Gradient Weight (0.0-1.0) - wpływ detekcji krawędzi

## Porady

### Kiedy używać poszczególnych algorytmów?

**Dokumenty tekstowe:**
- Sauvola Binarization (najlepszy wybór)
- Niblack Binarization

**Obrazy medyczne:**
- Phansalkar Binarization
- Adaptive Gradient Binarization (dla zachowania szczegółów)

**Obrazy biometryczne (odciski palców, tęczówka):**
- Adaptive Gradient Binarization ✨ (zachowuje krawędzie)
- Bernsen Binarization

**Obrazy z prostym tłem:**
- Otsu Binarization
- Kapur Binarization

**Obrazy z różnym oświetleniem:**
- Niblack, Sauvola lub Bernsen
- Adaptive Gradient Binarization ✨

### Dostosowanie parametrów

**Window Size (Rozmiar okna):**
- Mniejsze okno (3-11): Lepsze dla małych szczegółów
- Średnie okno (11-31): Uniwersalne zastosowanie
- Większe okno (31-51): Lepsze dla dużych obszarów

**K Parameter:**
- Niższe wartości: Bardziej konserwatywne progowanie
- Wyższe wartości: Bardziej agresywne progowanie

**Gradient Weight (tylko Adaptive Gradient):**
- 0.0-0.3: Subtełny wpływ krawędzi
- 0.3-0.6: Średni wpływ
- 0.6-1.0: Silny wpływ (może być zbyt mocny)

## Funkcje Pomocnicze

### Histogram
- Automatycznie aktualizowany po każdej operacji
- Dostępne kanały: Czerwony, Zielony, Niebieski, Średni
- Porównanie oryginału i przetworzonego obrazu

### Undo/Redo
- Przycisk "Undo" w pasku narzędzi
- Historia do 20 operacji
- Działa ze wszystkimi algorytmami

### Reset
- Przycisk "Reset" przywraca oryginalny obraz
- Czyści historię undo

## Rozwiązywanie Problemów

**Obraz jest zbyt ciemny/jasny po binaryzacji:**
- Dostosuj parametr K (Niblack/Sauvola/Phansalkar)
- Spróbuj innego algorytmu (np. Otsu zamiast Threshold)

**Zbyt dużo szumu:**
- Zwiększ rozmiar okna (Window Size)
- Użyj Sauvola zamiast Niblack
- Dla obrazów biometrycznych: Adaptive Gradient Binarization

**Utrata szczegółów krawędzi:**
- Użyj Adaptive Gradient Binarization ✨
- Zmniejsz Window Size
- Zwiększ Gradient Weight (w Adaptive Gradient)

**Długi czas przetwarzania:**
- Zmniejsz rozmiar obrazu przed załadowaniem
- Zmniejsz Window Size
- Algorytmy globalne (Otsu, Kapur, Li-Wu) są szybsze

## Eksport Wyników

Po zastosowaniu algorytmu:
1. Kliknij "Save"
2. Wybierz lokalizację
3. Wybierz format:
   - PNG - bezstratny, większy rozmiar
   - JPEG - skompresowany, mniejszy rozmiar

## Skróty Klawiszowe

(Do dodania w przyszłości)

---

**Wsparcie:** W razie problemów sprawdź plik `BINARIZATION_ALGORITHMS.md` z opisem algorytmów.

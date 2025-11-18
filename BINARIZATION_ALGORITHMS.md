# Zaimplementowane Algorytmy Binaryzacji

## Przegląd

Aplikacja zawiera implementację 8 zaawansowanych algorytmów binaryzacji obrazów, w tym 7 algorytmów z literatury naukowej oraz 1 autorski algorytm.

---

## 1. Niblack Binarization (Lokalna)

**Typ:** Binaryzacja lokalna adaptacyjna  
**Plik:** `NiblackBinarization.cs`

### Opis
Algorytm Niblacka wykorzystuje lokalną średnią i odchylenie standardowe do wyznaczania progu binaryzacji. Jest szczególnie skuteczny dla obrazów z nierównomiernym oświetleniem.

### Formuła
```
T(x,y) = m(x,y) + k × σ(x,y)
```
gdzie:
- `m(x,y)` - lokalna średnia
- `σ(x,y)` - lokalne odchylenie standardowe
- `k` - parametr regulujący wpływ odchylenia (domyślnie: -0.2)

### Parametry
- **k** (-1.0 do 0.5): Kontroluje czułość na kontrast lokalny
- **Window Size** (3-51): Rozmiar okna do obliczeń lokalnych

### Zastosowanie
Idealny dla dokumentów z cieniami i nierównomiernym oświetleniem.

---

## 2. Sauvola Binarization (Lokalna)

**Typ:** Binaryzacja lokalna adaptacyjna  
**Plik:** `SauvolaBinarization.cs`

### Opis
Udoskonalona wersja algorytmu Niblacka, zaprojektowana specjalnie dla obrazów dokumentów. Lepiej radzi sobie z różnymi tłami.

### Formuła
```
T(x,y) = m(x,y) × [1 + k × (σ(x,y)/R - 1)]
```
gdzie:
- `m(x,y)` - lokalna średnia
- `σ(x,y)` - lokalne odchylenie standardowe
- `k` - parametr regulujący (domyślnie: 0.5)
- `R` - zakres dynamiczny odchylenia standardowego (domyślnie: 128)

### Parametry
- **k** (0.0-1.0): Kontroluje wpływ odchylenia standardowego
- **Window Size** (3-51): Rozmiar okna analizy lokalnej

### Zastosowanie
Doskonały dla skanowanych dokumentów tekstowych z różnymi tłami.

**Referencja:** Sauvola, J., & Pietikäinen, M. (2000). "Adaptive document image binarization"

---

## 3. Phansalkar Binarization (Lokalna)

**Typ:** Binaryzacja lokalna adaptacyjna  
**Plik:** `PhansalkarBinarization.cs`

### Opis
Modyfikacja algorytmu Sauvola, zoptymalizowana dla obrazów o niskim kontraście, takich jak mikroskopowe obrazy medyczne.

### Formuła
```
T(x,y) = m(x,y) × [1 + p × exp(-q × m(x,y)) + k × (σ(x,y)/R - 1)]
```
gdzie:
- `p` - parametr ekspansji (domyślnie: 2.0)
- `q` - parametr eksponencjalny (domyślnie: 10.0)

### Parametry
- **k** (0.0-1.0): Parametr regulujący główny
- **Window Size** (3-51): Rozmiar okna lokalnego

### Zastosowanie
Idealne dla obrazów medycznych, mikroskopowych i innych obrazów o niskim kontraście.

**Referencja:** Phansalkar, N., et al. (2011). "Adaptive local thresholding for detection of nuclei in diversity stained cytology images"

---

## 4. Kapur Binarization (Globalna, Entropia)

**Typ:** Binaryzacja globalna oparta na entropii  
**Plik:** `KapurBinarization.cs`

### Opis
Algorytm wykorzystuje kryterium maksymalnej entropii do znalezienia optymalnego progu. Maksymalizuje sumę entropii tła i pierwszego planu.

### Zasada działania
1. Oblicza histogram obrazu i rozkład prawdopodobieństwa
2. Dla każdego możliwego progu oblicza entropię tła i pierwszego planu
3. Wybiera próg maksymalizujący sumę entropii

### Formuła entropii
```
H_total = H_background + H_foreground
```

### Parametry
Brak - algorytm automatycznie wyznacza optymalny próg.

### Zastosowanie
Doskonały dla obrazów z wyraźnym podziałem tła i obiektów, gdzie entropia jest dobrym kryterium separacji.

**Referencja:** Kapur, J. N., Sahoo, P. K., & Wong, A. K. (1985). "A new method for gray-level picture thresholding using the entropy of the histogram"

---

## 5. Li-Wu Binarization (Globalna, Entropia)

**Typ:** Binaryzacja globalna oparta na entropii krzyżowej  
**Plik:** `LiWuBinarization.cs`

### Opis
Algorytm wykorzystuje kryterium minimalnej entropii krzyżowej (minimum cross entropy) do wyznaczania progu. Minimalizuje różnicę między oryginalnym obrazem a zbinaryzowanym.

### Zasada działania
1. Oblicza średnie dla tła i pierwszego planu
2. Minimalizuje entropię krzyżową między obiema klasami
3. Uwzględnia wariancję wewnątrzklasową

### Parametry
Brak - algorytm automatycznie znajduje optymalny próg.

### Zastosowanie
Skuteczny dla obrazów, gdzie ważne jest minimalizowanie straty informacji podczas binaryzacji.

**Referencja:** Li, C. H., & Lee, C. K. (1993). "Minimum cross entropy thresholding"

---

## 6. Bernsen Binarization (Lokalna, Kontrast)

**Typ:** Binaryzacja lokalna oparta na kontraście  
**Plik:** `BernsenBinarization.cs`

### Opis
Algorytm wykorzystuje lokalny kontrast (różnicę między min i max w oknie) do wyznaczania progu. Jeśli kontrast jest zbyt niski, stosuje globalny próg.

### Formuła
```
if (max - min) < threshold_contrast:
    T = 128
else:
    T = (max + min) / 2
```

### Parametry
- **Window Size** (3-101): Rozmiar okna analizy
- **Contrast Threshold** (0-100): Minimalny kontrast do użycia progu lokalnego

### Zastosowanie
Doskonały dla obrazów z różnym oświetleniem, ale dobrze zdefiniowanymi krawędziami.

**Referencja:** Bernsen, J. (1986). "Dynamic thresholding of grey-level images"

---

## 7. Adaptive Gradient Binarization (WŁASNY ALGORYTM) ✨

**Typ:** Binaryzacja lokalna adaptacyjna z detekcją krawędzi  
**Plik:** `AdaptiveGradientBinarization.cs`

### Opis
**Autorski algorytm** łączący informację o gradiencie (krawędziach) z adaptacyjnym progowaniem lokalnym. Wykorzystuje operator Sobela do detekcji krawędzi, a następnie modyfikuje próg lokalny w zależności od siły gradientu.

### Innowacja
Algorytm łączy trzy elementy:
1. **Detekcję krawędzi (Sobel)** - wykrywa obszary z dużym gradientem
2. **Adaptacyjne progowanie** - bazuje na lokalnej średniej i odchyleniu standardowym
3. **Dynamiczną modyfikację progu** - obniża próg w obszarach krawędzi dla lepszej ich zachowania

### Formuła
```
gradient_factor = 1.0 - (gradient_weight × gradient_normalized)
T(x,y) = m(x,y) × gradient_factor × [1 + k × (σ(x,y)/128 - 1)]
```

### Parametry
- **Window Size** (3-51): Rozmiar okna analizy lokalnej
- **Gradient Weight** (0.0-1.0): Wpływ informacji o gradiencie na próg

### Zalety
- ✅ Lepsze zachowanie krawędzi niż tradycyjne metody
- ✅ Adaptuje się do lokalnych warunków oświetlenia
- ✅ Wykorzystuje informację przestrzenną (gradienty)
- ✅ Nie wymaga dodatkowego post-processingu

### Zastosowanie
Idealny dla obrazów wymagających zarówno dobrej binaryzacji jak i zachowania szczegółów krawędzi:
- Obrazy z teksturami
- Biometryczne obrazy (odciski palców, tęczówka)
- Obrazy z subtelnym konturem
- Obrazy wymagające późniejszej analizy kształtu

### Implementacja
Algorytm składa się z dwóch głównych faz:
1. **Pre-processing**: Obliczanie gradientów dla całego obrazu używając operatora Sobela
2. **Adaptive thresholding**: Dla każdego piksela obliczanie progu z uwzględnieniem lokalnego gradientu

---

## 8. Otsu Binarization (już istniała)

**Typ:** Binaryzacja globalna oparta na wariancji  
**Plik:** `OtsuBinarization.cs`

### Opis
Klasyczny algorytm minimalizujący wariancję wewnątrzklasową (lub maksymalizujący wariancję międzyklasową).

---

## Porównanie Algorytmów

| Algorytm | Typ | Złożoność | Parametry | Najlepsze dla |
|----------|-----|-----------|-----------|---------------|
| Niblack | Lokalna | O(n×w²) | 2 | Dokumenty z cieniami |
| Sauvola | Lokalna | O(n×w²) | 2 | Skanowane dokumenty |
| Phansalkar | Lokalna | O(n×w²) | 2 | Obrazy medyczne |
| Kapur | Globalna | O(n×256) | 0 | Wyraźny podział obiektów |
| Li-Wu | Globalna | O(n×256) | 0 | Minimalizacja straty informacji |
| Bernsen | Lokalna | O(n×w²) | 2 | Różne oświetlenie |
| **Adaptive Gradient** | **Lokalna** | **O(n×w²)** | **2** | **Zachowanie krawędzi** |
| Otsu | Globalna | O(n×256) | 0 | Ogólne zastosowanie |

gdzie:
- `n` - liczba pikseli
- `w` - rozmiar okna

---

## Implementacja w UI

Wszystkie algorytmy są dostępne w interfejsie użytkownika:
1. Wybierz obraz (przycisk "Open")
2. Wybierz algorytm z listy rozwijanej "Select Operation"
3. Dostosuj parametry (jeśli dostępne)
4. Kliknij "Apply Operation"
5. Użyj "Undo" aby cofnąć operację

---

## Punktacja

✅ **Niblack** - 25 pkt  
✅ **Sauvola** - 25 pkt  
✅ **Phansalkar** - 25 pkt  
✅ **Kapur** - 5 pkt (dodatkowy)  
✅ **Li-Wu** - 5 pkt (dodatkowy)  
✅ **Bernsen** - 5 pkt (dodatkowy)  
✅ **Adaptive Gradient (własny)** - 25 pkt  

**Razem: 115 punktów** (75 wymaganych + 40 dodatkowych)

---

## Bibliografia

1. Niblack, W. (1986). "An introduction to digital image processing"
2. Sauvola, J., & Pietikäinen, M. (2000). "Adaptive document image binarization"
3. Phansalkar, N., et al. (2011). "Adaptive local thresholding for detection of nuclei in diversity stained cytology images"
4. Kapur, J. N., et al. (1985). "A new method for gray-level picture thresholding using the entropy of the histogram"
5. Li, C. H., & Lee, C. K. (1993). "Minimum cross entropy thresholding"
6. Bernsen, J. (1986). "Dynamic thresholding of grey-level images"
7. Otsu, N. (1979). "A threshold selection method from gray-level histograms"

---

**Data implementacji:** 18 listopada 2025  
**Środowisko:** .NET 9.0, Avalonia UI  
**Autor:** Marcin Kondrat

# Файл LinkList.txt, объявление композиций

- ссылка на папку: `$str <название ссылки> <путь до этой папки>`
- ссылки должны быть объявлены прежде, чем будут использованы в пути.

Как работает ссылка?

- ссылка: `$str a1 Images\bg\`
- Объявление картинки: `$im black a1 + black`
- Ссылка на картинку в итоге: `Images\bg\black`
- Объявление звука: `$au black a1 + black`
- Объявление звука: `$au black a1 + black разделители FadeTime` - используется в подкоманде loop  
Пояснение: фрагменты считаются следующим образом - от начала трека до 1го разделителя - 1ый фрагмент. От 1го до 2го - 2ой фрагмент и т.д.
- Объявление покадровой анимации: `$anim black a1 + black`

!!!ВНИМАНИЕ, НЕ ОСТАВЛЯЙТЕ ПРОБЕЛЫ В НАЗВАНИЯХ ПАПОК В ПУТЯХ. ЭТО ВЕДЁТ К ОШИБКЕ!!!

# Файлы сценария (Scenario/*.txt)

## \#

Комментарии начинаются с `#`

## title

Титульный экран

    title [надпись] [надпись] ...

Количество надписей не ограничено, появляются одна за другой со стандартными таймингами.
Первая надпись немного больше.

## caption

Экран с текстом

    caption <isSimultaneous> <fontSize> <interval> <startDelay> <fadeInTime> <showTime> <captionsFadeOutTime> <screenFadeOutTime> [надпись] [надпись] ...

Пример:

    caption true 0.6 0.6 0 1.5 6 1.5 0 "Авторы" "" "Сергей Лапшин" "Максим Алексеев" "Дмитрий Баландин" "Алексей Карелин" "Владислав Алексеев" ""

То же, что и title, но с настройками.
- isSimultaneous - одновременное появление надписей, либо последовательное
- fontSize - размер шрифта
- interval - интервал между надписями
- startDelay - задержка перед появлением
- fadeInTime - время появления
- showTime - задержка перед изчезновением
- captionsFadeOutTime - время исчезновения надписей
- screenFadeOutTime - время исчезновения экрана

## label

метка

    label <имя метки>

все метки отображаются в специальном меню, вызываемом по клавише F1 и доступны оттуда для перехода

## sp, bg, cg

показать спрайт/БГ/ЦГ

    sp <имя композиции> [подкоманды]
    bg <имя композиции> [подкоманды]
    cg <имя композиции> [подкоманды]
    sp <имя спрайта> [одежда] [эмоция] ... [подкоманды]

Обычно имя композиции (определённое в LinkList) после создания объекта будет присвоено имени этого объекта, и в дальнейшем команды, обращающиеся к этому же объекту, будут изменять его, а не создавать новые.

    # Создание объекта
    bg prologue at BGLeft Time 0 with 2
    # Изменение анимации объекта
    bg prologue at BGCenter Time 3 with 0
    # Создание второго объекта с той же композицией
    bg prologue as pr2
    # Изменение композиции второго объекта
    bg Prologue_sword_ready as pr2

У команды sp есть некоторые особенности, упрощающие написание кода:  
для примера возьмём объявления композиций:

    $im er_smile_1 Images\sp\er\1 Images\sp\er\emot\smile
    $im er_sad_2 Images\sp\er\2 Images\sp\er\emot\sad

Знаки подчёркивания могут быть заменены на пробелы

    sp er smile 1

Имя объекта при этом будет определяться первым параметром, то есть **er**  
Имя объекта так же, как и всегда, можно переопределить подкомандой **as**
        
    sp er smile 1 as er2

После переопределения изменить объект можно двумя способами:

    # Обычный способ
    sp er sad 2 as er2
    # Только для sp
    sp er2 sad 2

Если нужно изменить не композицию, а что-то другое, например анимацию, можно написать только имя объекта (конечно, если он уже создан).

    sp er at Center normal
    sp er2 at Left far

## scene

`scene <sp|bg|cg> <...>` - перед показом очистить экран  
пример:

    scene bg black with 3

Пояснение: все объекты на сцене делятся на созданные до команды scene и после (включительно сам объект в команде scene).  
with, если применяется в команде scene, описывает переход не одного объекта, а всей старой сцены к новой.  
Старые объекты остаются неизменными во время перехода и удаляются мгновенно после него, в отличие от clear, удаляющей объекты по-отдельности.

## Подкоманды:

- `as <character's name>` - назначить спрайт определённому имени (применяется при необходимости 2-х одинаковых спрайтов), пример:
    - `sp li fuck shit as li2` - создать объект 1
    - `sp li2 ...`  - изменить объект 1
    - `sp li fuck2 shit2`  - создать объект 2

    Пишется всегда самой первой подкомандой.

- `set <composition>` - применить композицию
- `at <animation name> [параметр] [значение] ...` - применить анимацию (влияет на Replace переход с зацикленной анимации, при выполнении блока Replace вначале выполнится Show до цикла)  
  Параметры анимации пишутся после имени в формате имя_параметра значение имя_параметра значение и т.д..  
  Сами анимации пишутся на C#, с наследованием от базового класса ConcreteAnimation.
- `at previous` - не менять анимацию (влияет на Replace переход с зацикленной анимации, не будет рывка, Show не выполнится)   
- `with <time>` - дефолтная анимация перехода
- `with <material name> <time>` - кастомная анимация перехода
- `destroy <time>` - удалить объект, time - время анимации удаления
- `z <zlevel>` - порядок объектов на экране, меньше - ближе. cg, sp, bg выводятся в таком порядке с промежутком в 100 единиц, соответственно при больших значениях можно вывести спрайт поверх цг и т.д.

## Параметры анимаций:

Спрайтов:
- `XAnim true` (всегда вызывается так) - анимированное появление. 
- `XAnimHide true` - анимированное исчезновение, при использовании destroy
- `ThisScale` - разовое масштабирование для данного вызова
- `OffsetX`, `OffsetY` - Отступы по осям координат. Положительное число сдвигает вправо/вверх.
- `TimeShow` - скорость появления анимации
- `TimeReplace` - скорость изменения анимации в случае OnReplace  

У фонов возможны анимации at BGLeft, BGCenter, BGRight. Time определяет скорость анимации.
Для смещения фона нужно дополнительно использовать `with 0`, чтобы избежать дизолва фона самого в себя.

## mu, sfx, am

Звуки. поддерживается mp3 и wav.

    mu <name> volume <громкость> with <time>
    am <name> volume <громкость> with <time>
    sfx <name> volume <громкость> with <time>

time - время появления.  
Громкость измеряется от 0 до 1.  
`volume <громкость>` можно не указывать, стандартное значение 0.8.  
`with <time>` можно не указывать, стандартное значение 2 для mu и am, 0 для sfx.

Для вызова куска трека используется дополнительный параметр `loop <x>` (в соответствии с номером объявления). Используется для непрерывного перехода от части к части - трек начнётся сначала как обычно, затем как только доиграет до нужной части, будет играть её циклично. После этого можно указать другую часть, тогда трек продолжит играть с текущего момента до следующей части и дальше будет играть её. Последняя часть не зацикливается, трек перестанет играть, как только дойдёт до конца.

## effect

    effect [global] <add|remove> <effectName>

Добавляет или удаляет эффект со сцены.
примеры:

    effect add bluepeaks
    effect global add neonedge
	
Если есть global, эффект будет применяться поверх перехода между сценами и не будет исчезать при переходе.

## Другие команды

- `pause <time>` - пауза (можно пропустить кликом)
- `pauseblock <time>` - хардпауза (невозможно пропустить)
- `blockrollback` - запрет отматывать текст назад дальше текущей строки
- `blockrollforward` - запрет проматывать текст вперёд дальше текущей строки
- `clear [All/Images/Audio/bg/cg/sp/mu/sfx/am [time]]` - удаление определённой группы объектов  
  `clear` без параметров - то же самое, что и `clear all`  
  `time` по-умолчанию 1
- `jump <label>` - перейти к метке
- `nvl on` - включить режим nvl
- `nvl on left/right/center` - включить режим nvl в в определённой части экрана
- `nvl off` - отключить режим nvl
- `whide` - скрыть интерфейс
- `wshow` - показать интерфйс
- `blocknavi` - Блокировка навигации
- `unblocknavi` - Разблокировка навигации


# Файл ChList.txt, объявления персонажей:

    $ch <тег в сценарии> <имя в игре> <цвет> <обводка> <тень>

примеры:

    $ch dima Дмитрий #FF0000 false true  
    $ch er Эрин #FF0000 false true  
    $ch gg Сергей #B0B0B0 false true
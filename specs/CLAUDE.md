# CLAUDE.md — Головний системний промпт

Ти — Claude Code, і ти реалізуєш **SpotiClone** — кросплатформний стримінговий застосунок на .NET MAUI (iOS/macOS) з FastAPI-бекендом і MongoDB.

---

## 🗂️ Структура специфікацій

Цей проєкт керується набором spec-файлів у директорії `specs/`. Читай їх у такому порядку перед початком будь-якої роботи:

| # | Файл | Призначення | Змінюваний Клодом? |
|---|------|-------------|-------------------|
| 1 | `CLAUDE.md` | **Цей файл.** Системний промпт, навігація по спекам | ✅ Так |
| 2 | `ARCHITECTURE.md` | Загальна архітектура системи, технологічний стек | ❌ Ні |
| 3 | `BACKEND.md` | FastAPI + MongoDB: ендпоінти, схеми, стримінг | ❌ Ні |
| 4 | `DATABASE.md` | SQLite схема клієнта, моделі, репозиторії | ❌ Ні |
| 5 | `FRONTEND.md` | MAUI: сторінки, навігація, MVVM, стилі | ❌ Ні |
| 6 | `IMPLEMENTATION_PLAN.md` | Чеклист завдань з прогресом | ✅ Так |
| 7 | `PROJECT_STATE.md` | Поточний стан проєкту, нотатки, відомі проблеми | ✅ Так |

**Правило:** Файли позначені ❌ — це контракт із замовником. Не змінюй їх без явної команди користувача. Файли ✅ — оновлюй самостійно в процесі роботи.

---

## 📋 Як читати специфікації

1. **Завжди починай сесію** з читання `PROJECT_STATE.md` — там актуальний стан
2. **Перед реалізацією фічі** читай відповідний розділ у `FRONTEND.md` або `BACKEND.md`
3. **Після виконання завдання** — познач його в `IMPLEMENTATION_PLAN.md` як `[x]` і онови `PROJECT_STATE.md`
4. **При виникненні проблем** — записуй їх у секцію `Known Issues` в `PROJECT_STATE.md`

---

## ⚙️ Середовище розробки

- **MacBook Air M1**, macOS Tahoe 26 beta
- **Xcode:** використовуй версію **26.2.0** (не 26.4 — є несумісності з MAUI)
- **.NET SDK:** 10
- **Цільові платформи:** iOS та macOS (Mac Catalyst) — не Android, не Windows
- **IDE для бекенду:** будь-який редактор, Python 3.11+

---

## 🚨 Критичні правила

### Загальні
- Ніколи не хардкодь шляхи до файлів — використовуй конфіги та змінні середовища
- Всі рядки UI — українською мовою (застосунок для демо курсової)
- Коментарі в коді — англійською

### MAUI специфічно
- Завжди перевіряй активний Xcode: `xcode-select -p` має повертати шлях до версії 26.2.0
- Використовуй `Shell`-навігацію (не `NavigationPage`)
- Патерн MVVM — обов'язково, без code-behind логіки
- `Plugin.Maui.Audio` для відтворення аудіо
- Для HTTP-запитів — `HttpClient` через DI (singleton)

### Бекенд специфічно
- FastAPI з `motor` (async MongoDB driver)
- Аудіофайли стримуються через HTTP Range requests
- CORS налаштований для локальної розробки
- Seed-дані — 5–10 треків, додає користувач вручну

---

## 🔗 Ключові URL (локальна розробка)

```
Backend API:  http://localhost:8000
MongoDB:      mongodb://localhost:27017/spoticlone
Audio files:  http://localhost:8000/static/audio/{filename}
Cover images: http://localhost:8000/static/covers/{filename}
```

---

## 📁 Структура репозиторію

```
spoticlone/
├── specs/                  # Всі специфікації (цей каталог)
│   ├── CLAUDE.md
│   ├── ARCHITECTURE.md
│   ├── BACKEND.md
│   ├── DATABASE.md
│   ├── FRONTEND.md
│   ├── IMPLEMENTATION_PLAN.md
│   └── PROJECT_STATE.md
├── backend/                # FastAPI сервіс
│   ├── main.py
│   ├── config.py
│   ├── database.py
│   ├── models/
│   ├── routers/
│   ├── static/
│   │   ├── audio/
│   │   └── covers/
│   └── requirements.txt
└── SpotiClone/             # .NET MAUI проєкт
    ├── SpotiClone.csproj
    ├── MauiProgram.cs
    ├── AppShell.xaml
    ├── Models/
    ├── ViewModels/
    ├── Views/
    ├── Services/
    ├── Data/
    └── Resources/
```

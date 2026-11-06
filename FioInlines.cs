using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// Primo RPA C# Script
// Входная переменная: sourceText
// Выходная переменная: fioList

string inputText = sourceText;
List<string> fioList = new List<string>();

// Нормализация невидимых символов и пробелов
inputText = inputText ?? "";

inputText = inputText
    .Replace('\u00A0', ' ')
    .Replace('\u200B', ' ')
    .Replace('\u200C', ' ')
    .Replace('\u200D', ' ')
    .Replace('\uFEFF', ' ');

inputText = Regex.Replace(inputText, @"[ \t]+", " ");

// Маркер окончания номера договора
Regex markerRegex = new Regex(
    @"/(?:АН)?-ПД(?:-ДКП)?\s*(?<names>.*)$",
    RegexOptions.IgnoreCase
);

// Поиск ФИО перед номером договора
Regex prefixRegex = new Regex(
    @"ОДКП\s+(?<fio>(?:[А-ЯЁ][а-яёА-ЯЁ-]*\s+){1,3}[А-ЯЁ][а-яёА-ЯЁ-]*)\s+(?=[А-ЯЁ]{2,}[-/])"
);

// Проверка отдельного слова
Regex wordRegex = new Regex(
    @"^[А-ЯЁа-яё][А-ЯЁа-яё-]*$"
);

// Допустимые части составного отчества
string[] specialWords = new string[]
{
    "оглы",
    "кызы",
    "уулу"
};

// Разбиваем входной текст на строки
string[] lines = Regex.Split(inputText, @"\r\n|\r|\n");

foreach (string line in lines)
{
    List<string> candidates = new List<string>();

    // 1. ФИО после номера договора
    Match markerMatch = markerRegex.Match(line);

    if (markerMatch.Success)
    {
        string namesText = markerMatch.Groups["names"].Value;

        string[] fioParts = namesText.Split(
            new char[] { ',' },
            StringSplitOptions.RemoveEmptyEntries
        );

        foreach (string part in fioParts)
        {
            string candidate = part.Trim();

            candidate = candidate.Trim(
                ' ', '\t', '.', ';', ':', '(', ')', '[', ']'
            );

            string[] words = Regex.Split(candidate, @"\s+");

            bool valid = words.Length >= 2 && words.Length <= 4;

            if (valid)
            {
                foreach (string word in words)
                {
                    if (!wordRegex.IsMatch(word))
                    {
                        valid = false;
                        break;
                    }
                }
            }

            if (valid && words.Length == 4)
            {
                bool hasSpecialSuffix = false;

                foreach (string special in specialWords)
                {
                    if (String.Equals(
                        words[3],
                        special,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        hasSpecialSuffix = true;
                        break;
                    }
                }

                if (!hasSpecialSuffix)
                {
                    valid = false;
                }
            }

            if (valid)
            {
                candidates.Add(String.Join(" ", words));
            }
        }
    }

    // 2. ФИО перед номером договора
    Match prefixMatch = prefixRegex.Match(line);

    if (prefixMatch.Success)
    {
        string candidate = prefixMatch.Groups["fio"].Value.Trim();
        string[] words = Regex.Split(candidate, @"\s+");

        bool valid = words.Length >= 2 && words.Length <= 4;

        if (valid)
        {
            foreach (string word in words)
            {
                if (!wordRegex.IsMatch(word))
                {
                    valid = false;
                    break;
                }
            }
        }

        if (valid && words.Length == 4)
        {
            bool hasSpecialSuffix = false;

            foreach (string special in specialWords)
            {
                if (String.Equals(
                    words[3],
                    special,
                    StringComparison.OrdinalIgnoreCase))
                {
                    hasSpecialSuffix = true;
                    break;
                }
            }

            if (!hasSpecialSuffix)
            {
                valid = false;
            }
        }

        if (valid)
        {
            candidates.Add(String.Join(" ", words));
        }
    }

    // 3. Добавляем только уникальные ФИО, сохраняя порядок
    foreach (string candidate in candidates)
    {
        bool exists = false;

        foreach (string existingFio in fioList)
        {
            if (String.Equals(
                existingFio,
                candidate,
                StringComparison.OrdinalIgnoreCase))
            {
                exists = true;
                break;
            }
        }

        if (!exists)
        {
            fioList.Add(candidate);
        }
    }
}

// Результат для Primo RPA: fioList

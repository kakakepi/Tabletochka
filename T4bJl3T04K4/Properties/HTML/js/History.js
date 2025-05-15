document.addEventListener("DOMContentLoaded", function () {
    // Отправляем запрос на получение истории поиска при загрузке страницы
    getSearchHistory();

    // Обработка входящих сообщений из C#
    window.chrome.webview.addEventListener("message", event => {
        if (event.data.type === "searchHistoryData") {
            populateTable(event.data.data);
        }
        // Если необходимо, можно добавить обработку ошибок.
    });
});

// Функция для отправки запроса в C#
function getSearchHistory() {
    window.chrome.webview.postMessage({ action: "getSearchHistory" });
}

// Функция для заполнения таблицы данными
function populateTable(data) {
    const tbody = document.getElementById("historyBody");
    tbody.innerHTML = ""; // очищаем предыдущие записи, если они были

    data.forEach(entry => {
        const tr = document.createElement("tr");
        
        // Форматирование даты и времени
        const date = new Date(entry.searchDate);
        const day = String(date.getDate()).padStart(2, "0");
        const month = String(date.getMonth() + 1).padStart(2, "0");
        const year = date.getFullYear();
        const hours = String(date.getHours()).padStart(2, "0");
        const minutes = String(date.getMinutes()).padStart(2, "0");
        const formattedDate = `${day}.${month}.${year}<br>${hours}:${minutes}`;

        const tdDate = document.createElement("td");
        tdDate.innerHTML = formattedDate;

        const tdHistory = document.createElement("td");
        tdHistory.textContent = entry.searchHistoryText;

        tr.appendChild(tdDate);
        tr.appendChild(tdHistory);
        tbody.appendChild(tr);
    });
}

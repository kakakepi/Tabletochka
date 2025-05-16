document.addEventListener("DOMContentLoaded", function () {
  getSearchHistory();

  window.chrome.webview.addEventListener("message", event => {
    if (event.data.type === "searchHistoryData") {
      populateTable(event.data.data);
    }
  });

  const menuToggle = document.getElementById("menuToggle");
  menuToggle.addEventListener("click", togglePanel);

  const overlay = document.querySelector(".overlay");
  overlay.addEventListener("click", togglePanel);
});

function getSearchHistory() {
  window.chrome.webview.postMessage({ action: "getSearchHistory" });
}

function populateTable(data) {
  const tbody = document.getElementById("historyBody");
  tbody.innerHTML = ""; 
  data.forEach(entry => {
    const tr = document.createElement("tr");
    
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

function togglePanel() {
  const overlay = document.querySelector(".overlay");
  const panel = document.getElementById("sidePanel");
  if (overlay.style.display === "block") {
    overlay.style.display = "none";
    panel.style.right = "-300px";
  } else {
    overlay.style.display = "block";
    panel.style.right = "0";
  }
}

function toggleSubmenu(id) {
  const submenu = document.getElementById(id);
  submenu.style.display = submenu.style.display === "block" ? "none" : "block";
}
function logout() {
    window.chrome.webview.postMessage({ action: "logout" });
}

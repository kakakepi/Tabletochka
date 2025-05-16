document.addEventListener("DOMContentLoaded", function () {
    showTab("disease");
    getDiseaseList();
    getSymptomList();
});

// Панель управления
function togglePanel() {
    const panel = document.getElementById("sidePanel");
    const overlay = document.querySelector(".overlay");
    if (panel.style.right === "0px") {
        panel.style.right = "-300px";
        overlay.style.display = "none";
    } else {
        panel.style.right = "0px";
        overlay.style.display = "block";
    }
}

function toggleSubmenu(id) {
    const submenu = document.getElementById(id);
    submenu.style.display = submenu.style.display === "block" ? "none" : "block";
}

// Управление табами
function showTab(tabName) {
    document.querySelectorAll(".panel").forEach(panel => panel.style.display = "none");
    document.querySelectorAll(".tab").forEach(tab => tab.classList.remove("active"));
    
    if (tabName === "disease") {
        document.getElementById("diseasePanel").style.display = "block";
        document.getElementById("tabDisease").classList.add("active");
    } else {
        document.getElementById("symptomPanel").style.display = "block";
        document.getElementById("tabSymptom").classList.add("active");
    }
}

// Работа с болезнями
function getDiseaseList() {
    window.chrome.webview.postMessage({ action: "getDiseaseList" });
}

function renderDiseaseList(diseases) {
    const container = document.getElementById("diseaseList");
    container.innerHTML = diseases.map(d => {
        const escapedName = d.Name.replace(/'/g, "\\'");
        const escapedDesc = (d.Description || "Описание отсутствует").replace(/'/g, "\\'");
        const escapedSymptoms = d.SymptomIds.replace(/'/g, "\\'");

        return `
            <div class="record">
                <div>
                    <strong>${d.Name}</strong>
                    <p>${d.Description || "Описание отсутствует"}</p>
                </div>
                <div class="button-group">
                
                    <button type="button" onclick="editDisease('${d.Id}', '${escapedName}', '${escapedDesc}', '${escapedSymptoms}')">
                        Редактировать
                    </button>
                    <button type="button" onclick="deleteDisease('${d.Id}')">
                        Удалить
                    </button>
                </div>
            </div>
        `;
    }).join("");
}


function saveDisease() {
    const data = {
        id: document.getElementById("diseaseId").value,
        name: document.getElementById("diseaseName").value,
        description: document.getElementById("diseaseDescription").value,
        symptomIds: document.getElementById("diseaseSymptomIds").value.split(",").map(id => id.trim())
    };
    window.chrome.webview.postMessage({
        action: data.id ? "updateDisease" : "addDisease",
        ...data
    });
}

function resetDiseaseForm() {
    document.getElementById("diseaseForm").reset();
    document.getElementById("diseaseId").value = "";
}

function deleteDisease(id) {
    if (confirm("Удалить болезнь?")) {
        window.chrome.webview.postMessage({ action: "deleteDisease", id });
    }
}

// Работа с симптомами
function getSymptomList() {
    window.chrome.webview.postMessage({ action: "getSymptomList" });
}

function renderSymptomList(symptoms) {
    const container = document.getElementById("symptomList");
    container.innerHTML = symptoms.map(s => `
        <div class="record">
            <div>
                <strong>${s.Name}</strong>
            </div>
            <div class="button-group">
                <button type="button" onclick="editSymptom('${s.Id}', '${s.Name}', '${s.DiseaseId}')">Редактировать</button>
                <button type="button" onclick="deleteSymptom('${s.Id}')">Удалить</button>
            </div>
        </div>
    `).join("");
}

function saveSymptom() {
    const data = {
        id: document.getElementById("symptomId").value,
        name: document.getElementById("symptomName").value,
        diseaseId: document.getElementById("symptomDiseaseId").value
    };
    window.chrome.webview.postMessage({
        action: data.id ? "updateSymptom" : "addSymptom",
        ...data
    });
}

function resetSymptomForm() {
    document.getElementById("symptomForm").reset();
    document.getElementById("symptomId").value = "";
}

function deleteSymptom(id) {
    if (confirm("Удалить симптом?")) {
        window.chrome.webview.postMessage({ action: "deleteSymptom", id });
    }
}

// Обработчик сообщений
window.chrome.webview.addEventListener("message", event => {
    const msg = event.data;
    if (msg.type === "diseaseList") renderDiseaseList(msg.data);
    if (msg.type === "symptomList") renderSymptomList(msg.data);
    if (msg.type === "success") {
        alert(msg.message);
        getDiseaseList();
        getSymptomList();
    }
});

function onSystemChange() {
    const system = document.getElementById("systemSelect").value;
    if (system) {
        window.chrome.webview.postMessage({ 
            action: "getSymptomsBySystem", 
            system: system 
        });
    } else {
        getSymptomList(); // Показать все симптомы если система не выбрана
    }
}
function logout() {
    window.chrome.webview.postMessage({ action: "logout" });
}
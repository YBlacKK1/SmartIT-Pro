(() => {
    const root = document.documentElement;
    const savedTheme = localStorage.getItem("smartit-theme");
    const preferredDark = window.matchMedia?.("(prefers-color-scheme: dark)").matches;
    root.dataset.bsTheme = savedTheme || (preferredDark ? "dark" : "light");

    document.getElementById("themeToggle")?.addEventListener("click", () => {
        const next = root.dataset.bsTheme === "dark" ? "light" : "dark";
        root.dataset.bsTheme = next;
        localStorage.setItem("smartit-theme", next);
    });

    const sidebar = document.getElementById("appSidebar");
    const backdrop = document.getElementById("sidebarBackdrop");
    const closeSidebar = () => document.body.classList.remove("sidebar-open");
    document.getElementById("sidebarToggle")?.addEventListener("click", () =>
        document.body.classList.toggle("sidebar-open"));
    backdrop?.addEventListener("click", closeSidebar);
    sidebar?.querySelectorAll("a").forEach(link => link.addEventListener("click", closeSidebar));

    document.querySelectorAll("[data-table-search]").forEach(input => {
        const target = document.querySelector(input.dataset.tableSearch);
        if (!target) return;
        input.addEventListener("input", () => {
            const value = input.value.trim().toLocaleLowerCase();
            target.querySelectorAll("tbody tr").forEach(row => {
                row.hidden = value.length > 0 && !row.textContent.toLocaleLowerCase().includes(value);
            });
        });
    });

    document.querySelectorAll("[data-confirm]").forEach(element => {
        element.addEventListener("click", event => {
            if (!window.confirm(element.dataset.confirm || "Are you sure?")) event.preventDefault();
        });
    });

    setTimeout(() => {
        document.querySelectorAll(".app-alert:not(#liveNotification)").forEach(alert => alert.classList.add("fade-out"));
    }, 4500);

    if (window.signalR) {
        const notification = document.getElementById("liveNotification");
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/hubs/notifications")
            .withAutomaticReconnect()
            .build();

        connection.on("notification", message => {
            if (!notification) return;
            notification.textContent = message;
            notification.classList.remove("d-none");
            setTimeout(() => notification.classList.add("d-none"), 5000);
        });

        connection.start().catch(() => { /* Live notifications are optional. */ });
    }
})();

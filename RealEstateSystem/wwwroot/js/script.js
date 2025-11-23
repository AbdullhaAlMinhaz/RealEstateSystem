// ======================
// Mobile nav toggle
// ======================
const hamburger = document.getElementById("hamburger");
const nav = document.getElementById("nav");

if (hamburger && nav) {
    hamburger.addEventListener("click", () => {
        nav.classList.toggle("open");
    });

    // Close nav when a link is clicked (mobile UX)
    nav.querySelectorAll("a").forEach(link => {
        link.addEventListener("click", () => {
            nav.classList.remove("open");
        });
    });
}

// ======================
// Simulate "must be logged in" for search
// ======================
const searchForm = document.getElementById("searchForm");

if (searchForm) {
    searchForm.addEventListener("submit", function (e) {
        e.preventDefault();
        alert(
            "Search is available only after Login or Registration.\n\n" +
            "Later in backend you will check here if the user is authenticated."
        );
    });
}

// ======================
// Lock Details buttons until login (only alert, does NOT hide cards)
// ======================
document.querySelectorAll(".btn-details").forEach(btn => {
    btn.addEventListener("click", function (e) {
        e.preventDefault();
        alert(
            "You must Login or Register to view full property details.\n\n" +
            "Later this will redirect to Login page or show details if logged in."
        );
    });
});

// ======================
// CLIENT-SIDE PAGINATION FOR PROPERTY CARDS
// ======================
document.addEventListener("DOMContentLoaded", () => {
    const cards = Array.from(document.querySelectorAll(".property-card"));
    const paginationContainer = document.querySelector(".pagination");
    const pageSize = 6; // show 6 properties per page

    if (!cards.length || !paginationContainer) return;

    const totalPages = Math.ceil(cards.length / pageSize);
    let currentPage = 1;

    function showPage(page) {
        if (page < 1) page = 1;
        if (page > totalPages) page = totalPages;
        currentPage = page;

        cards.forEach((card, index) => {
            const pageIndex = Math.floor(index / pageSize) + 1;
            card.style.display = pageIndex === page ? "block" : "none";
        });

        // Update active button style
        paginationContainer.querySelectorAll(".pagination-btn").forEach(btn => {
            const btnPage = parseInt(btn.dataset.page, 10);
            btn.classList.toggle("active", btnPage === currentPage);
        });

        // Optional: scroll back to top of properties section
        const propertiesSection = document.getElementById("properties");
        if (propertiesSection) {
            propertiesSection.scrollIntoView({ behavior: "smooth", block: "start" });
        }
    }

    function buildPagination() {
        // Clear any buttons you may have hard-coded in HTML
        paginationContainer.innerHTML = "";

        for (let i = 1; i <= totalPages; i++) {
            const btn = document.createElement("button");
            btn.type = "button";
            btn.className = "pagination-btn";
            btn.dataset.page = i;
            btn.textContent = i;

            btn.addEventListener("click", () => {
                showPage(i);
            });

            paginationContainer.appendChild(btn);
        }
    }

    buildPagination();
    showPage(1);
});


// =============== MODAL HANDLING (Login & Register) ===============
function setupModal(triggerSelector, modalId, closeSelector) {
    const triggers = document.querySelectorAll(triggerSelector);
    const modal = document.getElementById(modalId);
    if (!modal || !triggers.length) return;

    const closes = modal.querySelectorAll(closeSelector);

    const open = () => {
        modal.classList.add("open");
        modal.setAttribute("aria-hidden", "false");
        document.body.classList.add("no-scroll");
    };

    const close = () => {
        modal.classList.remove("open");
        modal.setAttribute("aria-hidden", "true");
        document.body.classList.remove("no-scroll");
    };

    triggers.forEach(t => {
        t.addEventListener("click", e => {
            e.preventDefault();
            open();
        });
    });

    closes.forEach(c => {
        c.addEventListener("click", e => {
            e.preventDefault();
            close();
        });
    });

    document.addEventListener("keydown", e => {
        if (e.key === "Escape" && modal.classList.contains("open")) {
            close();
        }
    });
}

document.addEventListener("DOMContentLoaded", () => {
    setupModal(".js-open-login", "loginModal", ".js-close-login");
    setupModal(".js-open-register", "registerModal", ".js-close-register");
});

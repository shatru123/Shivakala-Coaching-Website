/* ===== SHIVKALA COACHING CLASSES — site.js ===== */

/* 1. Navbar active link highlight */
(function () {
    const path = window.location.pathname.toLowerCase();
    document.querySelectorAll('.sk-navbar .nav-link').forEach(link => {
        const href = (link.getAttribute('href') || '').toLowerCase().split('#')[0];
        if (href && href !== '/' && path.startsWith(href)) {
            link.classList.add('active-link');
        } else if (href === '/' && path === '/') {
            link.classList.add('active-link');
        }
    });
})();

/* 2. Navbar scroll shadow */
window.addEventListener('scroll', () => {
    const nav = document.querySelector('.sk-navbar');
    if (nav) nav.style.boxShadow = window.scrollY > 20
        ? '0 4px 24px rgba(27,43,107,0.18)'
        : '0 2px 8px rgba(27,43,107,0.10)';
}, { passive: true });

/* 3. Dark mode toggle */
const themeBtn = document.getElementById('themeToggle');
const html = document.documentElement;
const THEME_KEY = 'sk-theme';

function applyTheme(theme) {
    html.setAttribute('data-theme', theme);
    if (themeBtn) {
        themeBtn.innerHTML = theme === 'dark'
            ? '<i class="fa-solid fa-sun"></i>'
            : '<i class="fa-solid fa-moon"></i>';
        themeBtn.title = theme === 'dark' ? 'Switch to light mode' : 'Switch to dark mode';
    }
}

const saved = localStorage.getItem(THEME_KEY) || 'light';
applyTheme(saved);

if (themeBtn) {
    themeBtn.addEventListener('click', () => {
        const next = html.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
        localStorage.setItem(THEME_KEY, next);
        applyTheme(next);
    });
}

/* 4. Reveal on scroll (Intersection Observer) */
const revealObserver = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.classList.add('active');
            revealObserver.unobserve(entry.target);
        }
    });
}, { threshold: 0.12, rootMargin: '0px 0px -40px 0px' });

document.querySelectorAll('.reveal').forEach(el => revealObserver.observe(el));

/* 5. Auto-dismiss alerts after 5 s */
document.querySelectorAll('.glass-alert').forEach(el => {
    setTimeout(() => {
        el.style.transition = 'opacity 0.5s ease';
        el.style.opacity = '0';
        setTimeout(() => el.remove(), 500);
    }, 5000);
});

/* 6. Smooth scroll for anchor links */
document.querySelectorAll('a[href^="#"]').forEach(a => {
    a.addEventListener('click', e => {
        const id = a.getAttribute('href').slice(1);
        const target = id ? document.getElementById(id) : null;
        if (target) {
            e.preventDefault();
            target.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
    });
});

/* 7. Flow step hover glow */
document.querySelectorAll('.flow-step-icon').forEach(el => {
    el.addEventListener('mouseenter', () => { el.style.transform = 'translateY(-3px) scale(1.06)'; });
    el.addEventListener('mouseleave', () => { el.style.transform = ''; });
});

/* 8. Counter animation for stat cards */
function animateCount(el) {
    const target = parseInt(el.textContent.replace(/[^0-9]/g, ''), 10);
    if (isNaN(target) || target === 0) return;
    const suffix = el.textContent.replace(/[0-9]/g, '').trim();
    let start = 0;
    const step = Math.ceil(target / 40);
    const timer = setInterval(() => {
        start = Math.min(start + step, target);
        el.textContent = start + suffix;
        if (start >= target) clearInterval(timer);
    }, 30);
}

const statObserver = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            const strong = entry.target.querySelector('strong');
            if (strong) animateCount(strong);
            statObserver.unobserve(entry.target);
        }
    });
}, { threshold: 0.5 });

document.querySelectorAll('.stat-card, .admin-stat-card').forEach(el => statObserver.observe(el));

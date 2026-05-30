(() => {
  const root = document.documentElement;
  const storedTheme = localStorage.getItem('shivakala-theme');
  if (storedTheme) {
    root.setAttribute('data-theme', storedTheme);
  }

  const toggle = document.getElementById('themeToggle');
  if (toggle) {
    const updateIcon = () => {
      const isDark = root.getAttribute('data-theme') === 'dark';
      toggle.innerHTML = `<i class="fa-solid ${isDark ? 'fa-sun' : 'fa-moon'}"></i>`;
    };

    updateIcon();
    toggle.addEventListener('click', () => {
      const nextTheme = root.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
      root.setAttribute('data-theme', nextTheme);
      localStorage.setItem('shivakala-theme', nextTheme);
      updateIcon();
    });
  }

  const observer = new IntersectionObserver((entries) => {
    entries.forEach((entry) => {
      if (entry.isIntersecting) {
        entry.target.classList.add('active');
      }
    });
  }, { threshold: 0.12 });

  document.querySelectorAll('.reveal').forEach((element) => observer.observe(element));
})();

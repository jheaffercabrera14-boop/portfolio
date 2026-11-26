/* ---------- 1) Client-side check of the image-URL textarea (one URL per line) ----------
   Uses the SAME regex as the server (Patterns.ImageUrl), passed in data-pattern.
   The server checks again with Regex.IsMatch, so turning JavaScript off does not bypass it. */
document.addEventListener('submit', function (e) {
  var form = e.target;
  var box = form.querySelector('textarea[data-image-lines]');
  if (!box) return;
  var re = new RegExp(box.getAttribute('data-pattern'));
  var out = form.querySelector('.js-line-errors');
  var bad = [];
  box.value.split(/\r?\n/).forEach(function (line, i) {
    var t = line.trim();
    if (t === '') return;
    if (!re.test(t)) bad.push('Line ' + (i + 1) + ': "' + t + '" is not a valid http(s) URL.');
  });
  if (out) out.textContent = bad.join(' ');
  if (bad.length) { e.preventDefault(); box.focus(); }
}, true);

/* ---------- 2) Viewer: live search + course filter on the home page ---------- */
(function () {
  var grid = document.getElementById('project-grid');
  if (!grid) return;
  var cards = [].slice.call(grid.querySelectorAll('[data-course]'));
  var search = document.getElementById('project-search');
  var chips = [].slice.call(document.querySelectorAll('.chip[data-course]'));
  var empty = document.getElementById('no-results');
  var active = 'all';

  function apply() {
    var q = search ? search.value.toLowerCase().trim() : '';
    var shown = 0;
    cards.forEach(function (c) {
      var ok = (active === 'all' || c.getAttribute('data-course') === active) &&
               (q === '' || c.getAttribute('data-text').indexOf(q) > -1);
      c.hidden = !ok;
      if (ok) shown++;
    });
    if (empty) empty.hidden = shown !== 0 || cards.length === 0;
  }

  if (search) search.addEventListener('input', apply);
  chips.forEach(function (chip) {
    chip.addEventListener('click', function () {
      chips.forEach(function (c) { c.classList.remove('is-active'); });
      chip.classList.add('is-active');
      active = chip.getAttribute('data-course');
      apply();
    });
  });
})();

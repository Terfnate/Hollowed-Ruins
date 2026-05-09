const fs = require('fs');
const path = require('path');

const repoRoot = process.cwd();
const readmePath = path.join(repoRoot, 'README.md');
const outputPdf = path.join(repoRoot, 'README.pdf');
const tempHtml = path.join(repoRoot, 'README.extension-export.html');
const userProfile = process.env.USERPROFILE || process.env.HOME;
const extensionRoot = path.join(
  userProfile,
  '.vscode',
  'extensions',
  'deadpoulpe.custom-md-pdf-2.0.3'
);

function requireFromExtension(relPath) {
  return require(path.join(extensionRoot, relPath));
}

function read(relPath) {
  return fs.readFileSync(path.join(extensionRoot, relPath), 'utf8');
}

function fileExists(filePath) {
  try {
    fs.accessSync(filePath);
    return true;
  } catch {
    return false;
  }
}

function main() {
  if (!fileExists(readmePath)) {
    throw new Error(`README not found: ${readmePath}`);
  }
  if (!fileExists(extensionRoot)) {
    throw new Error(`Custom MD PDF extension not found: ${extensionRoot}`);
  }

  const source = fs.readFileSync(readmePath, 'utf8');

  const matter = requireFromExtension('node_modules/gray-matter')(source);
  const hljs = requireFromExtension('node_modules/highlight.js');
  const MarkdownIt = requireFromExtension('node_modules/markdown-it');
  const mdCheckbox = requireFromExtension('node_modules/markdown-it-checkbox');
  const mdEmoji = requireFromExtension('node_modules/markdown-it-emoji');
  const mdNamedHeaders = requireFromExtension('node_modules/markdown-it-named-headers');
  const mdContainer = requireFromExtension('node_modules/markdown-it-container');
  const mdInclude = requireFromExtension('node_modules/markdown-it-include');
  const Mustache = requireFromExtension('node_modules/mustache');
  const puppeteer = requireFromExtension('node_modules/puppeteer-core');

  const md = new MarkdownIt({
    html: true,
    breaks: true,
    highlight(str, lang) {
      if (lang && hljs.getLanguage(lang)) {
        try {
          str = hljs.highlight(lang, str, true).value;
        } catch {
          str = md.utils.escapeHtml(str);
        }
      } else {
        str = md.utils.escapeHtml(str);
      }
      return `<pre class="hljs"><code><div>${str}</div></code></pre>`;
    }
  });

  md.use(mdCheckbox);
  md.use(mdEmoji, {
    defs: require(path.join(extensionRoot, 'data', 'emoji.json'))
  });
  md.use(mdNamedHeaders, {
    slugify(value) {
      return encodeURI(
        value
          .trim()
          .toLowerCase()
          .replace(/\s+/g, '-')
          .replace(/[^\w\- ]+/g, '')
          .replace(/^\-+/, '')
          .replace(/\-+$/, '')
      );
    }
  });
  md.use(mdContainer, '', {
    validate(name) {
      return name.trim().length;
    },
    render(tokens, idx) {
      if (tokens[idx].info.trim() !== '') {
        return `<div class="${tokens[idx].info.trim()}">\n`;
      }
      return `</div>\n`;
    }
  });
  md.use(mdInclude, {
    root: repoRoot,
    includeRe: /:\[.+\]\((.+\..+)\)/i
  });

  const contentHtml = md.render(matter.content);

  const markdownCss = read('styles/markdown.css');
  const extensionCss = read('styles/custom-md-pdf.css');
  const printCss = fs.readFileSync(path.join(repoRoot, 'styles', 'readme-pdf.css'), 'utf8');
  const style = `<style>${markdownCss}\n${extensionCss}\n${printCss}</style>`;
  const template = read('template/template.html');

  const html = Mustache.render(template, {
    title: 'Hollowed Ruins README',
    style,
    content: contentHtml
  });

  fs.writeFileSync(tempHtml, html, 'utf8');

  exportPdf(puppeteer).catch((error) => {
    console.error(error);
    process.exitCode = 1;
  });
}

async function exportPdf(puppeteer) {
  const browser = await puppeteer.launch({
    executablePath: puppeteer.executablePath(),
    args: ['--no-sandbox', '--disable-setuid-sandbox']
  });

  try {
    const page = await browser.newPage();
    await page.goto(`file:///${tempHtml.replace(/\\/g, '/')}`, {
      waitUntil: 'networkidle0'
    });

    await page.pdf({
      path: outputPdf,
      format: 'A4',
      printBackground: true,
      displayHeaderFooter: true,
      headerTemplate: `
        <div style="width:100%;font-size:8px;padding:0 12mm;color:#6a5c51;display:flex;justify-content:space-between;font-family:'Segoe UI',Arial,sans-serif;">
          <span>HOLLOWED RUINS</span>
          <span>Player Manual & Project Overview</span>
        </div>`,
      footerTemplate: `
        <div style="width:100%;font-size:8px;padding:0 12mm;color:#6a5c51;display:flex;justify-content:space-between;font-family:'Segoe UI',Arial,sans-serif;">
          <span>CMPS434 Game Development Course Project</span>
          <span><span class="pageNumber"></span> / <span class="totalPages"></span></span>
        </div>`,
      margin: {
        top: '15mm',
        right: '12mm',
        bottom: '16mm',
        left: '12mm'
      }
    });
  } finally {
    await browser.close();
  }
}

main();

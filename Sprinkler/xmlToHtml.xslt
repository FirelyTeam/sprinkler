<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" doctype-system="about:legacy-compat" encoding="UTF-8" indent="yes" />

  <xsl:template match="Title" />

  <xsl:template match="TestResults">
    <html>
      <head>
        <title>
          <xsl:value-of select="Title" />
        </title>
        <style>
          body {font-family: Arial;}
          h1, h2 {color: sienna; margin-bottom: 0;}
          table {table-layout:fixed; border-collapse: collapse; width: 100%; border: 1px solid black;}
          th { border: 1px solid black; background-color: sienna; color: white; font-weight: bold;}
          tbody tr td { border-top: 1px solid black; border-left: 1px solid black; border-right: 1px solid black; }
          h1 {font-size: 15pt;}
          h2 {font-size: 14pt;}
          div {font: 10pt courier;white-space: pre-wrap;}
          .success { color: green;}
          .failure { color: red; font-weight: bold; }
          .stack { color: darkred; }
          .exception{ color: darkblue;}
          .exception .stack{display: none;}
          a.clickable:focus > .stack { display: block; }
        </style>
      </head>
      <body class="fhir-sprinkler">
        <h1>
          <xsl:value-of select="Title" />
        </h1>
        <xsl:apply-templates select="ResultList"/>
      </body>
    </html>
  </xsl:template>

  <xsl:template match="ResultList">
    <h2>Test results</h2>
    <table>
      <thead>
        <tr>
          <th>Category</th>
          <th>Code</th>
          <th>Title</th>
          <th>Outcome</th>
        </tr>
      </thead>
      <tbody>
        <xsl:apply-templates select="TestResult"/>
      </tbody>
    </table>
  </xsl:template>

  <xsl:template match="TestResult">
    <tr>
      <td class="category">
        <xsl:value-of select="Category" />
      </td>
      <td class="code">
        <xsl:value-of select="Code" />
      </td>
      <td class="title">
        <xsl:value-of select="Title" />
      </td>
      <td>
        <xsl:attribute name="class">
          outcome <xsl:choose>
          <xsl:when test="Outcome='Success'">success</xsl:when>
          <xsl:when test="Outcome='Success'">success</xsl:when>
          <xsl:when test="Outcome='Fail'">failure</xsl:when>
          <xsl:when test="Outcome='Skipped'">skipped</xsl:when>
          <xsl:otherwise>other</xsl:otherwise>
        </xsl:choose>
        </xsl:attribute>
        <xsl:value-of select="Outcome" />
      </td>
    </tr>
    <xsl:apply-templates select="Exception" />
  </xsl:template>

  <xsl:template match="Exception">
    <tr class="exception">
      <td colspan="4">
        <a class="clickable" tabindex="0">
          <xsl:if test=".//Stack">[+]</xsl:if>
          <xsl:apply-templates select="Message" />
          </a>
      </td>
    </tr>
  </xsl:template>

  <xsl:template match="Message">
    <xsl:if test="normalize-space(child::text()) != ''">
      <div class="message">
        <xsl:value-of select="normalize-space(child::text())" />
      </div>
    </xsl:if>
    <xsl:apply-templates select="Stack" />
    <xsl:apply-templates select="*/Message" />
  </xsl:template>

  <xsl:template match="Stack">
    <div class="stack">
      [#<xsl:value-of select="attribute::issue" />]<xsl:value-of select="child::text()" />
    </div>
  </xsl:template>
</xsl:stylesheet>
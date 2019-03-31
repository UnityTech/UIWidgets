using System;
using System.Collections.Generic;
using RSG;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;
using UnityEngine;
using DialogUtils = Unity.UIWidgets.material.DialogUtils;

namespace UIWidgetsGallery.gallery {
    public class Updater : StatefulWidget {
        public Updater(UpdateUrlFetcher updateUrlFetcher = null, Widget child = null, Key key = null)
            : base(key: key) {
            D.assert(updateUrlFetcher != null);
            this.updateUrlFetcher = updateUrlFetcher;
            this.child = child;
        }

        public readonly UpdateUrlFetcher updateUrlFetcher;
        public readonly Widget child;

        public override State createState() {
            return new UpdaterState();
        }
    }

    public class UpdaterState : State<Updater> {
        public override void initState() {
            base.initState();
            this._checkForUpdates();
        }

        static DateTime? _lastUpdateCheck;

        IPromise _checkForUpdates() {
            // Only prompt once a day
            if (_lastUpdateCheck != null &&
                (DateTime.Now - _lastUpdateCheck.Value).TotalDays < 1) {
                return Promise.Resolved(); // We already checked for updates recently
            }

            _lastUpdateCheck = DateTime.Now;

            return this.widget.updateUrlFetcher().Then(updateUrl => {
                if (updateUrl != null) {
                    return DialogUtils.showDialog(context: this.context, builder: this._buildDialog).Then(
                        result => {
                            if (result != null) {
                                bool wantsUpdate = (bool) result;
                                if (wantsUpdate) {
                                    Application.OpenURL(updateUrl);
                                }
                            }
                        });
                }

                return Promise.Resolved();
            });
        }

        Widget _buildDialog(BuildContext context) {
            ThemeData theme = Theme.of(context);
            TextStyle dialogTextStyle = theme.textTheme.subhead.copyWith(color: theme.textTheme.caption.color);
            return new AlertDialog(
                title: new Text("Update UIWidgets Gallery?"),
                content: new Text("A newer version is available.", style: dialogTextStyle),
                actions: new List<Widget>() {
                    new FlatButton(
                        child: new Text("NO THANKS"),
                        onPressed: () => { Navigator.pop(context, false); }
                    ),
                    new FlatButton(
                        child: new Text("UPDATE"),
                        onPressed: () => { Navigator.pop(context, true); }
                    )
                }
            );
        }

        public override Widget build(BuildContext context) {
            return this.widget.child;
        }
    }
}
